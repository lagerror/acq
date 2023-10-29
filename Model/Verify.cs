﻿using System.Net;

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
    public class Verify_Req
    {
        public string CardNo { set; get; }  //读者证号是 VARCHAR(32)
        public string OrderID { set; get; } //系统内部码订单号 是 VARCHAR(32)
        public List<Verify_item> ItemsList { set; get; }
    }

    public class Verify_item
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
     */
    public class Verify_Msg_Obj
    {
        public string OrderID { set; get; } // 系统内部码订单号 是 VARCHAR(32)
        public string BOOKID { set; get; } //系统内部码  是 Varchar(32) 审核结果 State 是 int 1:审核同意 2：审核驳回 3：待审核
        public string StateMsg { set; get; } //备注 StateMsg 否 Varchar 审核结果驳回时，必须填写驳回理由，在前台展示



    }
}