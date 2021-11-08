using Microsoft.Extensions.Logging;
using SapNwRfc;
using System;
using System.Data;

namespace SAPConnection
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始连接SAP!");
            //确保存在 SAP RFC SDK 二进制文件
            SapLibrary.EnsureLibraryPresent();
            //与 SAP 网络编织器连接
            string connectionString = "AppServerHost=172.16.101.207; SystemNumber=00; User=WMS03; Password=Aiko12345; Client=800; Language=ZH; PoolSize=10; Trace=20";
            using var connection = new SapConnection(connectionString);
            connection.Connect();

            // RFC 服务器通用处理程序
            sapConnection(connectionString);

            //没有输入或输出参数的呼叫功能
            using var someFunction = connection.CreateFunction("ZOAFM_HWYD_INFO");
            //someFunction.Invoke();

            //具有输入参数但没有输出参数的呼叫功能
            var result = someFunction.Invoke<SomeFunctionResult>(new SomeFunctionParameters
            {
                Budat = "2021-07-18",
                //Werks = "2000",
                //Aufnr = "",
                Matnr = "60001297",
                //Lgort = ""
            });
        }
        /// <summary>
        /// RFC 服务器通用处理程序
        /// </summary>
        public static void sapConnection(string connectionString)
        {
            //SapServer.InstallGenericServerFunctionHandler(connectionString, function =>
            //{
            //    switch (function.Name)
            //    {
            //        case "ZFM_WMS2_006":
            //            var parameters = function.GetParameters<SomeFunctionParameters>();
            //            function.SetResult(new SomeFunctionResult { Abc = "Some Value" });
            //            break;
            //    }
            //});
        }
        /// <summary>
        /// RFC 服务器
        /// </summary>
        public void Server(string connectionString)
        {
            using var server = SapServer.Create(connectionString);
            server.Launch();
        }
    }
    /// <summary>
    /// 通过创建从SapConnection 参数继承的类，可以添加其他连接参数：
    /// </summary>
    public class MySapConnectionParameters : SapConnectionParameters
    {
        [SapName("CST_PARAM")]
        public string CustomParameter { get; set; }
    }
    #region 具有输入和输出参数的呼叫功能
    class SomeFunctionParameters
    {
        /// <summary>
        /// 过账日期
        /// </summary>
        [SapName("IM_BUDAT")]
        public string Budat { get; set; }
        /// <summary>
        /// 工厂
        /// </summary>
        [SapName("IM_WERKS")]
        public string Werks { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        [SapName("IM_AUFNR")]
        public string Aufnr { get; set; }
        /// <summary>
        /// MATNR
        /// </summary>
        [SapName("IM_MATNR")]
        public string Matnr { get; set; }
        /// <summary>
        /// 库存地点
        /// </summary>
        [SapName("IM_LGORT")]
        public string Lgort { get; set; }
        /// <summary>
        /// 移动类型(库存管理)
        /// </summary>
        [SapName("IM_BWART")]
        public string Bwart { get; set; }
    }

    //class SomeFunctionResult
    //{
    //    /// <summary>
    //    /// 单字符标记
    //    /// </summary>
    //    [SapName("EX_CODE")]
    //    public string Code { get; set; }
    //    /// <summary>
    //    /// 文本字段长度 200
    //    /// </summary>
    //    [SapName("EX_MESSAGE")]
    //    public string Message { get; set; }
    //}
    #endregion
    #region 定义嵌套结构的模型

    //class SomeFunctionResult
    //{
    //    /// <summary>
    //    /// 单字符标记
    //    /// </summary>
    //    [SapName("EX_CODE")]
    //    public string Code { get; set; }
    //    /// <summary>
    //    /// 文本字段长度 200
    //    /// </summary>
    //    [SapName("EX_MESSAGE")]
    //    public string Message { get; set; }
    //    [SapName("OT_DATA")]
    //    public DataTable table { get; set; }
    //}

    //class SomeFunctionResultAddress
    //{
    //    /// <summary>
    //    /// 单字符标记
    //    /// </summary>
    //    [SapName("EX_CODE")]
    //    public string Code { get; set; }
    //    /// <summary>
    //    /// 文本字段长度 200
    //    /// </summary>
    //    [SapName("EX_MESSAGE")]
    //    public string Message { get; set; }
    //}
    #endregion


    #region 使用嵌套表定义模型
    class SomeFunctionResult
    {
        /// <summary>
        /// 单字符标记
        /// </summary>
        [SapName("EX_CODE")]
        public string Code { get; set; }
        /// <summary>
        /// 文本字段长度 200
        /// </summary>
        [SapName("EX_MESSAGE")]
        public string Message { get; set; }

        [SapName("OT_DATA")]
        public SomeFunctionResultItem[] Items { get; set; }
    }

    class SomeFunctionResultItem
    {
        public string MBLNR { get; set; }
        public string MJAHR { get; set; }
        public string ZEILE { get; set; }
        public string BUDAT { get; set; }
        public string WERKS { get; set; }
        public string BWART { get; set; }
        public string MATNR { get; set; }
        public string LGORT { get; set; }
        public string UMLGO { get; set; }
        public string CHARG { get; set; }
        public string LIFNR { get; set; }
        public string MENGE { get; set; }
        public string MEINS { get; set; }
        public string EBELN { get; set; }
        public string EBELP { get; set; }
        public string DMBTR { get; set; }
        public string ZJLFL { get; set; }
        public string NAME1 { get; set; }
        public string SORTL { get; set; }
        public string MATKL { get; set; }
        public string MAKTX { get; set; }
        public string AUFNR { get; set; }
        public string CPUDT { get; set; }
        public string CPUTM { get; set; }
        public string FEVOR { get; set; }
    }
    #endregion

    #region 将属性排除在映射中
    //class SomeFunctionParameters
    //{
    //    [SapIgnore]
    //    public string IgnoredProperty { get; set; }

    //    [SapName("SOME_FIELD")]
    //    public string SomeField { get; set; }
    //}

    //class SomeFunctionResult
    //{
    //    [SapIgnore]
    //    public string IgnoredProperty { get; set; }

    //    [SapName("SOME_FIELD")]
    //    public string SomeField { get; set; }
    //}
    #endregion
}
