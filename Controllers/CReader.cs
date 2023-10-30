﻿using acq.Model;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace acq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CReader : ControllerBase
    {
        private IConfiguration _configuration;
        private string connStr = "";
        public CReader(IConfiguration configuration) {
            _configuration = configuration;
            connStr = _configuration["Union:ConnStr"].ToString();
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
                        string result = "";
                        while (rd.Read())
                        {
                            Reader_Login_Msg_Obj obj=new Reader_Login_Msg_Obj();
                            obj.CardNo =  rd["xh"].ToString();
                            obj.UserName = rd["xm"].ToString();
                            obj.Department = rd["xqmc"] + "|" + rd["zymc"] + "|" + rd["bjmc"];
                            obj.Job = rd["xmmc"].ToString();
                        }
                        if (result == "")
                        {
                            msg.Code = 1;
                            msg.Result = "不存在可购买";
                        }
                        else
                        {
                            msg.Code = 1;
                            msg.Result = result;
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
