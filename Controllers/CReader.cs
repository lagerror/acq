using acq.Model;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;
using acq.Tools;
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
        private string signKey = "";
        public CReader(IConfiguration configuration) {
            _configuration = configuration;
            connStr = _configuration["Union:ConnStr"].ToString();
            signKey = _configuration["JDBook:SignValue"].ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CardNo">学号</param>
        /// <param name="PWD">密码</param>
        /// <returns></returns>
        [HttpGet]
        [Route("login")]
        public Msg Login(string CardNo= "2022006139", string PWD = "2022006139" )
        {

            Msg msg = new Msg();
            msg.Code = -1;
            Reader_Login_Req req = new Reader_Login_Req();
            req.CardNo = CardNo;
            req.PWD= PWD;
            
            //参数为空
            if(req==null | string.IsNullOrEmpty(req.CardNo) | string.IsNullOrEmpty(req.PWD))
            {
                msg.Result = String.Format("参数设置错误：{0}", "参数为空");
                return msg;
            }

            //读取数据库
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
                        OracleDataReader rd = comm.ExecuteReader();
                        //查询用户
                        if (rd.Read())
                        {
                            Reader_Login_Msg_Obj obj = new Reader_Login_Msg_Obj();
                            obj.CardNo = rd["xh"].ToString();
                            obj.UserName = rd["xm"].ToString();
                            obj.Department = rd["xqmc"] + "|" + rd["zymc"] + "|" + rd["bjmc"];
                            obj.Job = rd["xmmc"].ToString();
                            if (req.PWD == Tools.Tools.md5( rd["sfzjh"].ToString().Substring(12,6) + signKey))
                            {
                                msg.Code = 1;
                                msg.Result = "成功";
                                msg.Obj = obj;
                            }
                            else
                            {
                                msg.Code = -1;
                                msg.Result = "不匹配";
                                msg.Obj =null;

                            }
                        }
                        else   //没有查询到对应的用户
                        {
                            msg.Code = -1;
                            msg.Result = "没有此用户";
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
