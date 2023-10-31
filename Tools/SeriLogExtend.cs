using System.Data;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
//https://www.cnblogs.com/SaoJian/p/17509392.html
//https://www.cnblogs.com/fengchao1000/p/11811244.html
//https://github.com/serilog-mssql/serilog-sinks-mssqlserver
namespace acq.Tools
{
    public static class SeriLogExtend
    {
        public static void AddSerilLog(this ConfigureHostBuilder configureHostBuilder)
        {
            //输出模板
            string outputTemplate = "{NewLine}【{Level:u3}】{Timestamp:yyyy-MM-dd HH:mm:ss.fff}" +
                                    "{NewLine}#Msg#{Message:lj}" +
                                    "{NewLine}#Pro #{Properties:j}" +
                                    "{NewLine}#Exc#{Exception}" +
                                     new string('-', 50);

            // 配置Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // 排除Microsoft的日志
                .Enrich.FromLogContext() // 注册日志上下文
                .WriteTo.Console(outputTemplate: outputTemplate) // 输出到控制台
                .WriteTo.MSSqlServer("Server=10.4.3.201;Database=jdbookdb;User ID=jdbookdb_user;Password=jdbookdb_user_thisistest;TrustServerCertificate=true", sinkOptions: GetSqlServerSinkOptions(), columnOptions: GetColumnOptions())
                .WriteTo.Logger(configure => configure // 输出到文件
                            .MinimumLevel.Debug()
                            .WriteTo.File( //每天生成一个新的日志，按天来存日志
                                "logs\\{Date}-log.txt", //定输出到滚动日志文件中，每天会创建一个新的日志，按天来存日志
                                retainedFileCountLimit: 7,
                                outputTemplate: outputTemplate
                            ))
                .CreateLogger();

            configureHostBuilder.UseSerilog(Log.Logger); // 注册serilog

            /// <summary>
            /// 设置日志sqlserver配置
            /// </summary>
            /// <returns></returns>
            MSSqlServerSinkOptions GetSqlServerSinkOptions()
            {
                var sqlsinkotpions = new MSSqlServerSinkOptions();
                sqlsinkotpions.TableName = "Serilog";//表名称
                sqlsinkotpions.SchemaName = "dbo";//数据库模式
                sqlsinkotpions.AutoCreateSqlTable = true;//是否自动创建表
                return sqlsinkotpions;
            }

            /// <summary>
            /// 设置日志sqlserver 列配置
            /// </summary>
            /// <returns></returns>
            ColumnOptions GetColumnOptions()
            {
                var customColumnOptions = new ColumnOptions();

                customColumnOptions.Store.Remove(StandardColumn.MessageTemplate);//删除多余的这两列
                customColumnOptions.Store.Remove(StandardColumn.Properties);

                var columlist = new List<SqlColumn>();
                columlist.Add(new SqlColumn("IP", SqlDbType.NVarChar, true, 32));//添加一列，用于记录请求的IP
                columlist.Add(new SqlColumn("RequestJson", SqlDbType.NVarChar, true, 2000));//添加一列，用于记录请求参数string
                columlist.Add(new SqlColumn("ResponseJson", SqlDbType.NVarChar, true, 2000));//添加一列，用于记录响应数据
                customColumnOptions.AdditionalColumns = columlist;
                return customColumnOptions;
            }
        }
    }
}
