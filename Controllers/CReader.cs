using acq.Model;
using Microsoft.AspNetCore.Mvc;
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
            connStr = _configuration["Union.ConnStr"].ToString();
        }
        // GET: api/<CReader>
        [HttpGet]
        [Route("login")]
        public Msg Get([FromBody] string value)
        {

            Msg msg = new Msg();
            msg.Code = -1;
            Reader_Login_Req req = new Reader_Login_Req();
            //转换参数
            try 
            {
               req= Newtonsoft.Json.JsonConvert.DeserializeObject<Reader_Login_Req>(value);
            }
            catch(Exception ex)
            {
                msg.Result = String.Format("参数转换错误：{0}", ex.Message);
                return msg;
            }
            //参数为空
            if(req==null | string.IsNullOrEmpty(req.CardNo) | string.IsNullOrEmpty(req.Password))
            {
                msg.Result = String.Format("参数设置错误：{0}", "参数为空");
                return msg;
            }
            //读取数据库
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (SqlCommand comm = conn.CreateCommand())
                    {
                        string commStr = "select curlocal,barcode from holding left join biblios on biblios.bookrecno = holding.bookrecno where isbn = @isbn and holding.orglib = 'CD'";
                        comm.CommandText = commStr;
                        SqlParameter[] pars = new SqlParameter[] {
                        new SqlParameter("@isbn",req.CardNo)
                    };
                        SqlDataReader reader = comm.ExecuteReader();
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

        // GET api/<CReader>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<CReader>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CReader>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CReader>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
