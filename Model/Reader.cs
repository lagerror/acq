namespace acq.Model
{
    public class Login_Req
    {
        public string CardNo { get; set; }    //读者学号
        public string Password { get; set; } //md5（密码+key）加密
    }
    /*
     成功：{Code=1,Result=成功,Obj=usermodel}
     失败：{Code=-1,Result=密码错误,Obj=null}
     */

    public class Reader
    {
        public int CardNo { get; set; } //用户唯一标识
        public string UserName { get; set; }    //Varchar(32) 用户姓名
        public string? Department {set;get; } //读者所在院系
        public string? Job { get; set; }  //读者职务
        public string? Phone { get; set; } //手机号
    }
}
