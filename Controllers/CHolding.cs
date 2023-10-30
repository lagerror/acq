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
        // GET: api/<CHolding>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<CHolding>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
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
                        string commStr = "select curlocal,barcode from holding left join biblios on biblios.bookrecno = holding.bookrecno where isbn = :isbn and holding.orglib = 'CD'";
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

        // PUT api/<CHolding>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CHolding>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
