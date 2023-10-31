using acq.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data.OracleClient;
using Oracle.ManagedDataAccess.Client;
using System.Security.Cryptography;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace acq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CHolding : ControllerBase
    {
        private IConfiguration _configuration;
        private string connStr;
        private string signKey = "";
        private string orgId = "";
        public CHolding(IConfiguration configuration) { 
            _configuration = configuration;
            connStr=_configuration["Interlib:ConnStr"].ToString();
            signKey = _configuration["JDBook:SignValue"].ToString();
            orgId = _configuration["JDBook:orgId"].ToString();
        }
        /*
         {
            "ISBN":"9787106054168",
            "BookName":"bookname"
          }
        7276184DC005B99B79B5814BC8F05D1C
        3239
         */
        [HttpPost]
        [Route("Exist")]
        public Msg Post(string orgId, string sign,Holding_Exist_Req req)
        {
            Msg msg = new Msg();
            msg.Code = -1;
            
            if (req == null || string.IsNullOrEmpty(req.ISBN))
            { 
                msg.Code = -1;
                msg.Result = "参数或者ISBN为空";
                return msg;
            }
            //鉴权
            if (sign != Tools.Tools.md5(orgId + req.ISBN + signKey))
            {
                msg.Result = "无效的授权";
                return msg;
            }
            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (OracleCommand comm = conn.CreateCommand())
                    {
                        //title author pubdate publisher price page recno bookrecno orglib orglocal state barcode callno

                        string commStr = "select  biblios.title,biblios.author,biblios.pubdate,biblios.publisher,biblios.price, biblios.page, holding.recno,holding.bookrecno,holding.orglib,holding.orglocal,holding.state,holding.barcode,holding.callno from bib_isbnidx " +
                                            "left join biblios on bib_isbnidx.bookrecno = biblios.bookrecno " +
                                            "left join holding on biblios.bookrecno = holding.bookrecno " +
                                                "where bib_isbnidx.isbn = :isbn";

                        comm.CommandText = commStr;
                        OracleParameter[] pars = new OracleParameter[] {
                            new OracleParameter("isbn",req.ISBN)
                        };
                        comm.Parameters.AddRange(pars);
                        OracleDataReader reader = comm.ExecuteReader();
                        string result = "";
                        List<Holding_Detail> res = new List<Holding_Detail>();
                        while (reader.Read())
                        {
                            Holding_Detail detail = new Holding_Detail();
                            //title author pubdate publisher price page recno bookrecno orglib orglocal state barcode callno
                            detail.title = reader.GetString(0);
                            detail.author   = reader.GetString(1);
                            detail.pubdate = reader.GetString(2);
                            detail.publisher = reader.GetString(3);
                            detail.price = reader.GetString(4);
                            detail.page = reader.GetString(5);
                            detail.recno    = reader.GetString(6);
                            detail.bookrecno = reader.GetString(7);
                            detail.orglib = reader.GetString(8);
                            detail.orglocal = reader.GetString(9);
                            detail.state = reader.GetString(10);
                            detail.barcode = reader.GetString(11);
                            detail.callno = reader.GetString(12);
                            res.Add(detail);
                            result += String.Format("条码：{0}；馆藏地：{1};索取号：{2} | ", reader["barcode"].ToString(), reader["orglocal"].ToString(), reader["callno"].ToString());
                        }
                        if (result == "")
                        {
                            msg.Code = 1;
                            msg.Result = "不存在可购买";
                            msg.Obj = null;
                        }
                        else
                        {
                            msg.Code = -1;
                            msg.Result = "图书馆已有馆藏 "+ result + " 您可以前往指定点借阅";
                            msg.Obj = res;
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                msg.Code = -1;
                msg.Result = String.Format("数据库操作异常：{0}", ex.Message);

            }
            return msg;
        }

       
    }
}
