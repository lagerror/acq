using acq.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data.OracleClient;
using Oracle.ManagedDataAccess.Client;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace acq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CHolding : ControllerBase
    {
        private IConfiguration _configuration;
        private string connStr;
        public CHolding(IConfiguration configuration) { 
            _configuration = configuration;
            connStr=_configuration["Interlib:ConnStr"].ToString();
        }
        // POST api/<CHolding>
        [HttpPost]
        [Route("Exist")]
        public Msg Post([FromBody] string value)
        {
            Msg msg = new Msg();
            Holding_Exist_Req req=null;
            try
            {
                req = Newtonsoft.Json.JsonConvert.DeserializeObject<Holding_Exist_Req>(value);

            }catch(Exception ex)
            {
                msg.Code = -1;
                msg.Result =String.Format( "解析参数错误：{0}",ex);
                return msg;
            } 
            
            if (req == null || string.IsNullOrEmpty(req.ISBN))
            { 
                msg.Code = -1;
                msg.Result = "参数或者ISBN为空";
                return msg;
            }

            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (OracleCommand comm = conn.CreateCommand())
                    {
                        ////title author pubdate publisher price page recno bookrecno orglib orglocal state

                        string commStr = "select  biblios.title,biblios.author,biblios.pubdate,biblios.publisher,biblios.price, biblios.page, holding.recno,holding.bookrecno,holding.orglib,holding.orglocal,holding.state from bib_isbnidx " +
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
                        while (reader.Read())
                        {
                            result += String.Format("条码：{0}；馆藏地：{1} \r\n", reader["barcode"].ToString(), reader["curlocal"].ToString());
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
