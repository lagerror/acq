using Microsoft.Extensions.Hosting;
using System.Net;

namespace acq.Model
{
    /*
     5、下单通知接口（图书管理系统提供）
         内部码 BOOKID 是 Varchar(32 ) 是否需要加密由图书馆提出加密方式
    
         图书管理系统返回参数
            返回状态码 Code 是 int 返回结果状态码, 1:成功接收 如果没有返回 1，系统将定时继续推送返回信息 
            Result 是 Varchar(32 ) 返回结果信息
            返回数据源 Obj 否 Object 数据实体模型 是否需要加密由图书馆提出加密方式
                 列举如下
                 例：成功：{Code=1,Result=已接收,Obj=null}
                 失败：{Code=-1, Result = 未接收, Obj = null}
     */
    public class Order_Notice_Req
    {
        public string BOOKID { get; set; }  //
    }
    /*
     * 6、订单签收通知（图书管理系统提供）
         是否需要加密由图书馆提出加密方式
     */
    public class Order_Sign_Req
    {
        public string BOOKID { set; get; } // 系统内容部码 BOOKID 是 Varchar(32)
        public string STATE { set; get; } // 签收状态 STATE 是 Varchar(32) 签收类型(1-签收,2-异常(退货等没有签收的))
        public string  Date { set; get; }  //签收时间 Date 是 Varchar(32) 签收时间
    }
    /*
     修改后的订单签收通知并借阅到读者证上
     */
    public class Order_sign_loan_Req
    {
        public string certId { set; get; }
        public string signDate { set; get; }
        public string? batch { set; get; }
        public string? bookId { set; get; }
        public string? isbn { set; get; }
        public string? bookName { set; get; }
        public string? author { set; get; }
        public string? publisher { set; get; }
        public string? pubTime    { set; get;}
        public string? price { set; get; }
        public string? remark { set; get; }
        public string? phone { set; get; }
    }
    /*
     7、订单下单接口（汇采平台平台提供）
     */
    public class Order_Place_Req
    {
        public string  TokenID {set;get; } //TokenID TokenID 是 由汇采平台提供
        public string TokenPWD { set; get; } //TokenPWD TokenPWD 是 由汇采平台提供
        public string BOOKID { set; get; }  //系统内容部码 BOOKID 是 Varchar(32)
        public string STATE { set; get; } //1：审核通过可以下单，2 审核不通过状态为 
        public string? OrderValid { set; get; } //2 是必须传入不通过原因
        public string TokenSgin {  set; get; } // 是否需要加密由汇采平台提出加密方式
    }

    /*
     8、订单查询接口（汇采平台平台提供）
     */
    public class Order_Search_Req
    { 
        public string TokenID { set; get; } // TokenID TokenID 是 由汇采平台提供
        public string TokenPWD { set; get; } //TokenPWD TokenPWD 是 由汇采平台提供
        public String BOOKID { set; get; } // 系统内容部码 BOOKID 是 Varchar(32)
        public string TOKENSgin { set; get; } //TokenSgin TokenSgin 是 是否需要加密由汇采平台提出加密方式
    }
    /// <summary>
    /// 订单查询后返回结果中的MSG
    /// </summary>
    public class Order_Search_Res_Msg_Obj
    { 
        public string BOOKID { set; get; } //系统内部码BOOKID 是 Varchar(32)
        public int OrderState { set; get; } //订单状态OrderState是 int 0:新建 1:妥投 2：拒收
        public int  OrderValid { set; get; } //订单有效性OrderValid是 Int 0:无效 1：有效
        public string OrderSubmitTime { set; get; } //订单提交时间OrderSubmitTime是 Varchar
        public string OrderGetTime { set; get; }  //订单签收时间OrderGetTime否 Varchar
        public string StateMsg { set; get; }     //备注 StateMsg否 Varchar 审核结果驳回时，必须填写驳回理由，在前台展示

    }
}

