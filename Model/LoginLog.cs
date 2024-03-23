namespace acq.Model
{
    public class LoginLog
    {
        private IConfiguration _configuration;
        private string connStr = "";
        
        public LoginLog(IConfiguration configuration) {
            _configuration = configuration;
            connStr = _configuration["LocalDB:MssqlConnection"].ToString();
         }
        public int id { set; get; }
        public string UserName { set; get; }    
        public string PassWord { set; get; }
        public string OrgId { set; get; }
        public DateTime dt { set; get; }

        public int Insert(LoginLog loginLog)
        {
            int Ret = 0;

            return Ret;
        }

    }
}
