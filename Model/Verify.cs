using System.Net;

namespace acq.Model
{
    /*
     {
CardNo:1234,
OrderID:20191122,
itemList:[
{
BOOKID:1,
BookName:计算机算法，
Author:,
ISBN:978720226951,
Publish:清华大学出版社,
电话：400-838-0150 地址：北京市朝阳区天畅园 5 号楼 10 层 - 7 -
PublishTime:2011-12-01，
Price:139
Jsprice:118.2
},
{
BOOKID:2,
BookName:数学之美，
Author:吴军,
ISBN:9787115373557,
Publish:人民邮电出版社,
PublishTime:2014-11-01
Price:49
Jsprice:41
}
]
}
     */
    /*3、图书购验证/审核证接口（图书管理系统提供）*/

    public class Verify_Req
    {
        public string CardNo { set; get; }  //读者证号是 VARCHAR(32)
        public string OrderID { set; get; } //系统内部码订单号 是 VARCHAR(32)
        public List<Verify_Req_Item> ItemsList { set; get; }
    }

    public class Verify_Req_Item
    {
        public string BOOKID { set; get; } //系统内部码 是 Varchar(32)
        public string ISBN { set; get; } //ISBN 是 Varchar(32)书籍 ISBN 码
        public string BookName { set; get; }//书名 是 Varchar(512) 书名
        public string? Author { set; get; } //作者  否 Varchar(512) 作者
        public string? Publish { set; get; } //出版社  否 Varchar(512)出版社
        public string? PublishTime { set; get; }//出版时间 否 Varchar(32) 出版时间
        public string price { set; get; }    //码洋 是 Varchar(32)
        public string Jsprice { set; get; }   //结算价 Jsprice 是 Varchar
    }
    /*
     Msg.obj
    成功：{Code=1,Result=验证完成,Obj=ResItem}
    失败：{Code=-1,Result=读者证已停用,Obj=null}
     */
    public class Verify_Res_Msg_Obj
    {
        public string OrderID { set; get; } // 系统内部码订单号 是 VARCHAR(32)
        public List<Verify_Res_Msg_Obj_Item> ItemsList { set; get; }
    }
    public class Verify_Res_Msg_Obj_Item
    {
        public string BOOKID { set; get; } //系统内部码  是 Varchar(32) 审核结果
        public string State { set; get; }   //State 是 int 1:审核同意 2：审核驳回 3：待审核
        public string? StateMsg { set; get; } //备注 StateMsg 否 Varchar 审核结果驳回时，必须填写驳回理由，在前台展示

    }
    /* 4、图书管理系统审核结果查询（图书管理系统提供)*/

    public class Verify_Search_Req
    { 
        public  string BOOKID { set; get; } //系统内部码，可支持多个查询，参数以“，”号隔开？
    }

    public class Verify_Search_Res_Msg_Obj
    { 
        List<Verify_Res_Msg_Obj_Item> ResItem { set; get; }
    }
}
