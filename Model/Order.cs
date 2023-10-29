using Microsoft.Extensions.Hosting;
using System.Net;

namespace acq.Model
{
    /*
     5、下单通知接口（图书管理系统提供）

 部码 BOOKID 是 Varchar(32 ) 是否需要加密由图书馆提出加密方式
 
    图书管理系统返回参数
  返回状态码 Code 是 int 返回结果状态码, 1:成功接收 如果没有返回 1，系统将定时继续推 送
 反回信息 Result 是 Varchar(32 ) 返回结果信息
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
Url:http:xxx.xxx.xxx.xxx:port
提交方式：POST
汇采平台请求参数
字段名 变量名 必填 类型 描述

是否需要加密由图书馆提出加密方
式*
     */
    public class Order_Sign_Req
    {
        public string BOOKID { set; get; } // 系统内容部码 BOOKID 是 Varchar(32)
        public string STATE { set; get; } // 签收状态 STATE 是 Varchar(32) 签收类型(1-签收,2-异常(退货等没有签收的))
        public string  Date { set; get; }  //签收时间 Date 是 Varchar(32) 签收时间

    }
}
