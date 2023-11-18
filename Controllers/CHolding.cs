using acq.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data.OracleClient;
using Oracle.ManagedDataAccess.Client;
using System.Security.Cryptography;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace acq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CHolding : ControllerBase
    {
        private IConfiguration _configuration;
        private string connStr;
        private string signKey = "";
        private string orgId = "";
        public CHolding(IConfiguration configuration) { 
            _configuration = configuration;
            connStr=_configuration["Interlib:ConnStr"].ToString();
            signKey = _configuration["JDBook:SignValue"].ToString();
            orgId = _configuration["JDBook:orgId"].ToString();
        }
        /*9787506365284
         * B034BB2F6B50CF5CC41D3C63A7EAF9F6
         {9787010253671(有预定)
        7FF454266071B4FB60157E79F3C9B350
         9787106054168",（有馆藏）
        7276184DC005B99B79B5814BC8F05D1C
        3239
         */
        [HttpPost]
        [Route("Exist")]
        public Msg Post(string orgId, string sign,Holding_Exist_Req req)
        {
            Msg msg = new Msg();
            msg.Code = -1;
            
            if (req == null || string.IsNullOrEmpty(req.ISBN))
            { 
                msg.Code = -1;
                msg.Result = "参数或者ISBN为空";
                return msg;
            }
            //鉴权
            if (sign != Tools.Tools.md5(orgId + req.ISBN + signKey))
            {
                msg.Result = "无效的授权";
                return msg;
            }
            bool isHolding = false;
            bool isVendor_order = false;

            try
            {
                using (OracleConnection conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    using (OracleCommand comm = conn.CreateCommand())
                    {
                        //title author pubdate publisher price page recno bookrecno orglib orglocal state barcode callno

                        string commStr = "select  biblios.title,biblios.author,biblios.pubdate,biblios.publisher,biblios.price, biblios.page, holding.recno,holding.bookrecno,holding.orglib,holding.orglocal,holding.state,holding.barcode,holding.callno from bib_isbnidx " +
                                            "left join biblios on bib_isbnidx.bookrecno = biblios.bookrecno " +
                                            "left join holding on biblios.bookrecno = holding.bookrecno " +
                                                "where bib_isbnidx.isbn = :isbn and holding.recno is not null";

                        comm.CommandText = commStr;
                        OracleParameter[] pars = new OracleParameter[] {
                            new OracleParameter("isbn",req.ISBN)
                        };
                        comm.Parameters.AddRange(pars);
                        OracleDataReader reader = comm.ExecuteReader();
                        string result = "";
                        List<Holding_Detail> res = new List<Holding_Detail>();
                        while (reader.Read())
                        {
                            isHolding=true;
                            Holding_Detail detail = new Holding_Detail();
                            //title author pubdate publisher price page recno bookrecno orglib orglocal state barcode callno
                            detail.title = reader.GetString(0);
                            detail.author   = reader.GetString(1);
                            detail.pubdate = reader.GetString(2);
                            detail.publisher = reader.GetString(3);
                            detail.price = reader.GetString(4);
                            detail.page = reader.GetString(5);
                            detail.recno    = reader.GetString(6);
                            detail.bookrecno = reader.GetString(7);
                            detail.orglib = reader.GetString(8);
                            detail.orglocal = reader.GetString(9);
                            detail.state = reader.GetString(10);
                            detail.barcode = reader.GetString(11);
                            detail.callno = reader.GetString(12);
                            res.Add(detail);
                            result += String.Format("条码：{0}；馆藏地：{1};索取号：{2} | ", reader["barcode"].ToString(), reader["orglocal"].ToString(), reader["callno"].ToString());
                        }
                        //没有馆藏再查预定
                        if (!isHolding)
                        {
                            //查询征订单
                            //title	author isbn pubdate publisher price page recno liblocal ordertime copies 
                            
                            commStr="select book_vendor.title,book_vendor.author,book_vendor.isbn,book_vendor.pubdate,book_vendor.publisher,book_vendor.price,book_vendor.page,book_vendor.recno,book_order.liblocal,book_order.ordertime,book_order.copies from bookvendor_isbnidx "+ 
                                    "left join book_vendor on bookvendor_isbnidx.recno= book_vendor.recno "+
                                    "left join book_order on book_vendor.recno=book_order.bookvendorrecno " +
                                    "where bookvendor_isbnidx.fieldcontent=:isbn and book_order.liblocal is not null";
                            comm.CommandText = commStr;
                            reader=comm.ExecuteReader();
                            while (reader.Read())
                            {
                                isVendor_order=true;
                                Holding_Detail detail = new Holding_Detail();
                                detail.title = reader.GetString(0);  
                                detail.author = reader.GetString(1);
                                detail.publisher = reader.GetString(4);
                                detail.price = reader.GetString(5);
                                detail.page = reader.GetString(6);
                                detail.bookrecno= reader.GetString(7);  //recno
                                detail.orglocal= reader.GetString(8);    //liblocal 订购分配数量和地点
                                detail.orglib= reader.GetString(9);     //订购时间
                                detail.state= reader.GetString(10);     //订购本数
                                res.Add(detail);
                                result += String.Format("预定情况：{0}；预定时间：{1}  | ", detail.orglocal, detail.orglib);

                            }
                            //有预定
                            if(isVendor_order) 
                            { 
                                msg.Code = -1;
                                msg.Result = "图书馆已经预定此书 " + result + "请等待图书到馆加工上架";
                                msg.Obj = res;
                            }
                        }
                        //有馆藏
                        else
                        {
                            msg.Code = -1;
                            msg.Result = "图书馆已有馆藏 "+ result + " 您可以前往指定点借阅";
                            msg.Obj = res;
                        }
                    }
                    conn.Close();
                }
                //没有馆藏，没有预定 可以购买
                if (!isHolding & !isVendor_order)
                {
                    msg.Code = 1;
                    msg.Result = "没有馆藏和预定，可以购买";
                    msg.Obj = null;
                }
            }
            catch (Exception ex)
            {
                msg.Code = -1;
                msg.Result = String.Format("数据库操作异常：{0}", ex.Message);
            }
            
            return msg;
        }

       
    }
}
