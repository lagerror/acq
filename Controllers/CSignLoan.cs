using acq.Model;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sql;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.IdentityModel.Tokens;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace acq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CSignLoan : ControllerBase
    {
        private IConfiguration _configuration;
        private string connStr = "";
        private string connStrReader = "";
        private string connStrJD = "";
        private string signKey = "";
        private string orgId = "";
        private Serilog.ILogger _logger;
        public CSignLoan(IConfiguration configuration, Serilog.ILogger logger)
        {
            _configuration = configuration;
            connStr = _configuration["Union:ConnStr"].ToString();
            connStrReader = _configuration["Interlib:ConnStr"].ToString();
            connStrJD = _configuration["LocalDB:MssqlConnection"].ToString();
            signKey = _configuration["JDBook:SignValue"].ToString();
            orgId = _configuration["JDBook:orgId"].ToString();
            _logger = logger;

        }
        [HttpPost]
        [Route("sign")]
        public Msg Post(string orgId, string sign, Order_sign_loan_Req req)
        {
            Msg msg=new Msg();
            msg.Code = -1;
            //鉴权
            if (sign != Tools.Tools.md5(orgId + req.certId + signKey))
            {
                msg.Result = "无效的授权";
                return msg;
            }
            //根据读者证号查询读者校区，以便分配不同的校区条码号，以便对应还回（无法实现，因为教职工没区分）

            //将借阅记录插入，并修改馆藏状态
            try
            {
                //查询馆藏库，锁定一个没有分配的条码号
                using (OracleConnection conn = new OracleConnection(connStrReader))
                {
                    conn.Open();
                    using (OracleTransaction trans = conn.BeginTransaction())
                    {
                        using (OracleCommand comm = conn.CreateCommand())
                        {
                            int flag = 0;
                            //分配一个在馆条码
                            //string commStr = "select barcode from holding where state=2 and orglocal='A22'  and donor is null order by barcode asc";
                            //comm.CommandText = commStr;
                            //string barcode = comm.ExecuteScalar().ToString();
                            
                            //调整了逻辑，直接从云端返回条码号
                            if (string.IsNullOrEmpty(req.bookId))
                            {
                                msg.Result = "获取的条码号为空";
                                return msg;
                            }
                            string barcode = req.bookId;
                            
                            //更新条码为借出状态
                            string volinfo =req.isbn+"|" +req.phone+"|"+ req.bookName + "|" + req.price + "|" + req.author+"|"+req.publisher + "|" + req.pubTime+"|"+req.remark;
                            
                            if (volinfo.Length > 100)
                            {
                                volinfo = volinfo.Substring(0, 100);
                            }
                            string  commStr = "update holding set state=3,donor='李靖',volinfo='"+volinfo +"' where barcode='" + barcode + "'";
                            comm.CommandText = commStr;
                            //更新错误则回滚
                            if (comm.ExecuteNonQuery() != 1)
                            {
                                flag=1;
                            }
                            //插入到loan_work
                            commStr = "insert into loan_work(rdid, barcode, rulestate, loancount, loandate, returndate) values(:rdNo, :barcode, 1, 0, sysdate, sysdate + 30)";
                            comm.CommandText = commStr;
                            comm.Parameters.Clear();
                            OracleParameter[] pars1 = new OracleParameter[] {
                                new OracleParameter("rdNo",req.certId.Trim()),
                                new OracleParameter("barcode",barcode)
                            };
                            //插入loan_work错误则回滚
                            comm.Parameters.AddRange(pars1);
                            if (comm.ExecuteNonQuery() != 1)
                            {
                                flag = 3;
                            }

                            //插入到LOG_CIR
                            commStr = "insert into log_cir(logtype, libcode, userid, ipaddr, data1, data2, data3, data4, return_time) values('30001', 'CD', 'admin', '10.203.1.211', '0', :rdNo, :barcode, 'A22', sysdate + 30)";
                            comm.CommandText = commStr;
                            comm.Parameters.Clear();
                            OracleParameter[] pars = new OracleParameter[] {
                                new OracleParameter("rdNo",req.certId.Trim()),
                                new OracleParameter("barcode",barcode)
                            };
                            //插入LOG_CIR错误则回滚
                            comm.Parameters.AddRange(pars);
                            if (comm.ExecuteNonQuery() != 1)
                            {
                                flag = 2;
                            }

                           
                            //如果没有错误则提交，否则回滚
                            if (flag == 0) 
                            {
                                trans.Commit();
                                msg.Code = 1;
                                msg.Result = "订单签收后借出成功";
                            }
                            else
                            {
                                trans.Rollback();
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                msg.Code = -1;
                msg.Result= ex.Message;
            }
            _logger
                    .ForContext("RequestJson", string.Format("{0}", System.Text.Json.JsonSerializer.Serialize(req, new JsonSerializerOptions()
                    {
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    })))
                   .ForContext("ResponseJson", string.Format("{0}", System.Text.Json.JsonSerializer.Serialize(msg, new JsonSerializerOptions()
                   {
                       Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                   })))
                   .Warning("订单签收后借出");
            return msg;
        }
    }
}
