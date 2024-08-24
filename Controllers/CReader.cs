using acq.Model;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sql;
using acq.Tools;
using Serilog;
using Azure.Core;
using Newtonsoft.Json;
using Serilog.Debugging;
using System.Reflection.Metadata.Ecma335;
//   string SignValue = "z02rw4JfsgXA1GUh";
//   string orgid = "3239";

namespace acq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CReader : ControllerBase
    {
        private IConfiguration _configuration;
        private string connStr = "";
        private string connStrReader = "";
        private string connStrJD="";
        private string signKey = "";
        private string orgId = "";
        private Serilog.ILogger _logger;
        private string passwordGlobal = "";
        
        public CReader(IConfiguration configuration,Serilog.ILogger logger) {
            _configuration = configuration;
            connStr = _configuration["Union:ConnStr"].ToString();
            connStrReader = _configuration["Interlib:ConnStr"].ToString();
            connStrJD = _configuration["LocalDB:MssqlConnection"].ToString();
            signKey = _configuration["JDBook:SignValue"].ToString();
            orgId= _configuration["JDBook:orgId"].ToString();
            _logger = logger;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CardNo">学号 = "2022006139" 9667ABDE5CB42C3B3E963F34EA7AA109 3239 E46473FB7628E5DB8D3265EC7552B14E</param>
        /// <param name="PWD">密码</param>
        /// <returns></returns>
        [HttpGet]
        [Route("login")]
        public Msg Login(string CardNo, string PWD ,string orgId,string sign)
        {
            
            Msg msg = new Msg();
            msg.Code = -1;
            if (orgId == "shareBook")
            {
                signKey = "LibSharefsgXA1GUh";
            }
            //鉴权
            if (sign != Tools.Tools.md5(orgId + CardNo + signKey))
            {
                msg.Result = "无效的授权";
                _logger.ForContext("RequestJson", string.Format("{0}", CardNo))
                    .ForContext("ResponseJson", string.Format("{0}", orgId))
                    .Warning(msg.Result);
                return msg;
            }

            Reader_Login_Req req = new Reader_Login_Req();
            req.CardNo = CardNo;
            req.PWD= PWD;
            
            //参数为空
            if(req==null | string.IsNullOrEmpty(req.CardNo) | string.IsNullOrEmpty(req.PWD))
            {
                msg.Result = String.Format("参数设置错误：{0}", "参数为空");
                return msg;
            }
            //5次密码错误将锁定1小时，并记录错误级别日志
            try
            { 
                using(SqlConnection conn=new SqlConnection(connStrJD))
                {
                    conn.Open();
                    
                    using(SqlCommand command=conn.CreateCommand())
                    {

                        string commstr = "select count(*) from serilog where requestjson=@requestJson and message='密码不匹配' and Level='Warning' and DATEDIFF(\"MI\",TimeStamp, GETDATE())<60";
                        command.CommandText = commstr;
                        SqlParameter[] pars = new SqlParameter[]
                        {
                            new SqlParameter("@requestJson",req.CardNo)
                        };
                        command.Parameters.AddRange(pars);
                        if ((int)command.ExecuteScalar() > 2)
                        {
                            msg.Result = string.Format("密码错误三次以上,请在一小时后再登录或联系图书馆!");
                            _logger.ForContext("RequestJson", string.Format("{0}", CardNo))
                                .ForContext("ResponseJson", string.Format("{0}", orgId))
                                .Error(msg.Result);
                            return msg;
                        }

                    }
                    conn.Close();

                }
            } 
            catch(Exception ex)
            {
                msg.Result=string.Format("本地数据库错误：{0}", ex.Message);
                return msg;
            }

            //读取图书馆读者库判断是否存在读者并返回密码字段passwordGlobal用于验证
            try
            {
                using (OracleConnection conn = new OracleConnection(connStrReader))
                {
                    conn.Open();
                    using (OracleCommand comm = conn.CreateCommand())
                    {

                        string commStr = "select  rdpasswd from reader where rdid=:rdid and rdcfstate='1'";
                        comm.CommandText = commStr;
                        OracleParameter[] pars = new OracleParameter[] {
                                            new OracleParameter(":rdid",req.CardNo)
                                        };
                        comm.Parameters.AddRange(pars);
                        //在读者数据库没有查到状态正常的用户
                        object password = comm.ExecuteScalar();

                        if (password == DBNull.Value || password == null || password.ToString() == "" || password.ToString().Length<5)
                        {
                            msg.Code = -1;
                            msg.Result = string.Format("没有 {0} 用户或者密码强度不够，请联系图书馆！", req.CardNo);
                            return msg;
                        }
                        else
                        {
                            passwordGlobal = password.ToString();
                            //只有在此条件下继续读取读者信息
                            if (req.PWD != Tools.Tools.md5(passwordGlobal + signKey))
                            {
                                msg.Result = string.Format("密码不匹配", req.CardNo);
                                _logger.ForContext("RequestJson", string.Format("{0}", CardNo))
                                        .ForContext("ResponseJson", string.Format("{0}", orgId))
                                        .Warning(msg.Result);
                                return msg;
                            }
                        }


                    }
                    conn.Close();
                }
            }
            catch (Exception ex)

            {
                msg.Code = -1;
                msg.Result = string.Format("读者数据库操作异常：{0}", ex.Message);
                return msg ;
            }

            //读取学校数据库获取用户详细信息
            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (OracleCommand comm = conn.CreateCommand())
                    {   //'2022006139'

                            string commStr = "select xh,xm,sfzjh,xqmc,zymc,bjmc,xmmc from usr_zsj.v_xx_xsxx where xh=:xh and sfzx='是'";
                            comm.CommandText = commStr;
                            OracleParameter[] pars = new OracleParameter[] {
                                            new OracleParameter(":xh",req.CardNo)
                                        };
                         comm.Parameters.AddRange(pars);
                        OracleDataReader rd = null;
                         //查询学校数据库中学生   
                        if (req.CardNo.Length > 6)
                        {   
                            rd = comm.ExecuteReader();
                            //查询用户
                            if (rd.Read())
                            {
                                Reader_Login_Msg_Obj obj = new Reader_Login_Msg_Obj();
                                obj.CardNo = rd["xh"].ToString();
                                obj.UserName = rd["xm"].ToString();
                                obj.Department ="学生"+ "|"+ rd["xqmc"] + "|" + rd["zymc"] + "|" + rd["bjmc"];
                                obj.Job = rd["xmmc"].ToString();
                                msg.Code = 1;
                                msg.Obj = obj;
                            }
                            else   //没有查询到对应的用户
                            {
                                msg.Code = -1;
                                msg.Result = "学校数据库中没有此用户";
                            }
                        }
                        //查询学校数据库中教工
                        else
                        {
                            commStr = "select zgh,xm,dwmc,sfzjh from usr_zsj.v_tsg_jzgxx WHERE zgh=:xh";
                            comm.CommandText = commStr;
                            rd = comm.ExecuteReader();
                            if (rd.Read())
                            {
                                Reader_Login_Msg_Obj obj = new Reader_Login_Msg_Obj();
                                obj.CardNo = rd["zgh"].ToString();
                                obj.UserName = rd["xm"].ToString();
                                obj.Department = "教职工|" + rd["dwmc"] ;
                                obj.Job = "教职工";
                                msg.Code = 1;
                                msg.Obj = obj;
                            }
                            else
                            {
                                msg.Code = -1;
                                msg.Result = "学校数据库中没有此用户";
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                msg.Code = -1;
                msg.Result = String.Format("学校数据库操作异常：{0}", ex.Message);
            }
            
            return msg;
        }
    }
}
