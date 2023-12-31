﻿using Microsoft.VisualBasic;

namespace acq.Model
{
    /*Msg.Result
        {Code=1,Result=不存在可购买,Obj=null}
        {Code = -1,Result = 图书在 XX 图书馆 2 楼 A 区，可前往借阅,Obj = null}
    */
    /// <summary>
    /// 2、图书馆藏验证(图书管理系统提供)
    /// 由北端查询是否图书馆中有此本图书
    /// 返回MSG结果；
    /// </summary>
    public class Holding_Exist_Req
    {
       public string ISBN { set; get; } // ISBN 是 Varchar(32)书籍 ISBN 码
       public string BookName { set; get; }   //书名 是 Varchar(512)书名
       public string? Author { set; get; } //作者 否 Varchar(512) 作者
       public string? Publish { set; get; } //出版社  否 Varchar(512) 出版社
       public string?  PublishTime { set; get; } //出版时 否 Varchar(32)
    }
    public class Holding_Detail
    {
        //title author pubdate publisher price page recno bookrecno orglib orglocal state barcode
        public string title { set; get; }
        public string author { set; get; }
        public string pubdate { set; get; }
        public string publisher { set; get; }
        public string price { set; get; }
        public string page { set; get; }
        public string recno { set; get; }
        public string bookrecno { set; get; }
        public string orglib { set; get; }
        public string orglocal { set; get; }
        public string state { set; get; }
        public string barcode { set; get; }
        public string callno { set; get; }
    }
}
