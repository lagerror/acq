namespace acq.Model
{
    public class Msg
    {
        public int Code { get; set; } //返回结果状态码
        public string Result { get; set; } //返回结果信息 Varchar(32)
        public dynamic? Obj { get; set; }
    }
}
