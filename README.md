# sapdotnetconnector
use sapnrrc and sapdotnetconnector 、.net5.0、.net framwork4 to connect sap function.
1.1．	net5
1.2．	安装SapNwRfc
dotnet add package SapNwRfc  or
PM> Install-Package SapNwRfc
1.3将saplib文件夹下的文件拷贝到项目bin文件夹下
1.4连接到SAP服务器
	string connectionString = "AppServerHost=MY_SERVER_HOST; SystemNumber=00; User=MY_SAP_USER; Password=SECRET; Client=100; Language=EN; PoolSize=5; Trace=8";

using var connection = new SapConnection(connectionString);
connection.Connect();
1.5建立Sap 接口连接，参数为接口名
using var someFunction = connection.CreateFunction("BAPI_SOME_FUNCTION_NAME");
1.6调取接口并返回数据
	//具有输入参数但没有输出参数的呼叫功能
  var result = someFunction.Invoke<SomeFunctionResult>(new SomeFunctionParameters
  {
      Budat = "2021-07-18",
      //Werks = "2000",
      //Aufnr = "",
      Matnr = "60001297",
      //Lgort = ""
  });
1.7接口相关对象
	class SomeFunctionParameters
  {
      [SapName("SOME_FIELD")]
      public string SomeField { get; set; }
  }
  class SomeFunctionResult
  {
      [SapName("RES_ABC")]
      public string Abc { get; set; }

      [SapName("RES_ITEMS")]
      public SomeFunctionResultItem[] Items { get; set; }
  }

  class SomeFunctionResultItem
  {
      [SapName("ITM_NAME")]
      public string Name { get; set; }
  }
2.0 .net framework 4.0
2.1 安装SAPDotNetConnector3
	dotnet add package SAPDotNetConnector3	or
  PM> Install-Package SAPDotNetConnector3
2.2 连接对象
	public abstract class Common
  {
      /// <summary>
      /// 获取SAP配置信息
      /// </summary>
      public static SAPServerInfoModel GetSapServerInfo()
      {
          SAPServerInfoModel sapInfo = new SAPServerInfoModel
          {
              NAME = ConfigurationManager.AppSettings["NAME"],
              ASHOST = ConfigurationManager.AppSettings["ASHOST"],
              CLIENT = ConfigurationManager.AppSettings["CLIENT"],
              USER = ConfigurationManager.AppSettings["USER"],
              PASSWD = ConfigurationManager.AppSettings["PASSWD"],
              SYSNR = ConfigurationManager.AppSettings["SYSNR"],
              LANG = ConfigurationManager.AppSettings["LANG"]
          };
          return sapInfo;
      }
  }
2.3连接参数
	public class SAPConfiguration : IDestinationConfiguration
  {
      public bool ChangeEventsSupported()
      {
          return false;
      }

      public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

      private SAPServerInfoModel info;

      public SAPConfiguration(SAPServerInfoModel info)
      {
          this.info = info;
      }

      public RfcConfigParameters GetParameters(string destinationName)
      {
          try
          {
              if (destinationName.Equals("PRD"))
              {
                  RfcConfigParameters parms = new RfcConfigParameters();
                  parms.Add(RfcConfigParameters.Name, info.NAME);
                  parms.Add(RfcConfigParameters.AppServerHost, info.ASHOST);
                  parms.Add(RfcConfigParameters.Client, info.CLIENT);
                  parms.Add(RfcConfigParameters.User, info.USER);
                  parms.Add(RfcConfigParameters.Password, info.PASSWD);
                  parms.Add(RfcConfigParameters.Language, info.LANG);
                  parms.Add(RfcConfigParameters.SystemNumber, info.SYSNR);
                  return parms;
              }
              else
              {
                  return null;
              }
          }
          catch (Exception ex)
          {
              return null;
          }
      }
  }
  public class SAPServerInfoModel
  {
      public string NAME { get; set; }
      public string ASHOST { get; set; }
      public string CLIENT { get; set; }
      public string USER { get; set; }
      public string PASSWD { get; set; }
      public string SYSNR { get; set; }
      public string LANG { get; set; }
  }
2.5创建连接
	public class SAPServerHelper : IDisposable
  {
      private RfcDestination prd;//SAP Rfc定义声明对象

      private SAPConfiguration sapConfig;//SAP配置对象

      #region 创建SAP连接对象
      /// <summary>
      /// 创建SAP连接对象
      /// </summary>
      public SAPServerHelper()
      {
          SAPServerInfoModel sapInfo = Common.GetSapServerInfo();
          sapConfig = new SAPConfiguration(sapInfo);
          prd = RfcDestinationManager.GetDestination(sapConfig.GetParameters("PRD"));
      }
      #endregion

      #region 与SAP进行数据交互（MES->SAP && SAP->MES） 
      /// <summary>
      /// 与SAP进行数据交互（MES->SAP && SAP->MES）
      /// </summary>
      /// <param name="funName">SAP接口名</param>
      /// <param name="lstParameters">SAP传入参数列表</param>
      /// <param name="TableName">SAP表名</param>
      /// <param name="rtbInputList">SAP传入参数Table列表</param>
      /// <param name="outParameters">返回值列表</param>
      /// <param name="outList">返回参数列表</param>
      /// <returns></returns>
      public DataTable getSapFunctionToTable(string funName, Dictionary<string, string> lstParameters, string TableName,
          Dictionary<IRfcTable, string> rtbInputList, out Dictionary<string, string> outParameters, List<string> outList)
      {
          Dictionary<string, string> outDataList = new Dictionary<string, string>();

          DataTable dt = new DataTable();
          RfcRepository repo = prd.Repository;
          //输入
          IRfcFunction function = repo.CreateFunction(funName);
          foreach (var item in lstParameters)
          {
              function.SetValue(item.Key, item.Value);
          }
          foreach (var item in rtbInputList)
          {
              function.SetValue(item.Value, item.Key);
          }
          function.Invoke(prd);

          //输出
          IRfcTable rtb = function.GetTable(TableName);
          foreach (var item in outList)
          {
              outDataList[item] = function.GetString(item);
          }
          outParameters = outDataList;
          dt = CreateTable(rtb, TableName);
          return dt;
      }
      #endregion

      #region 与SAP进行数据交互（MES->SAP）返回Table
      /// <summary>
      /// 
      /// </summary>
      /// <param name="funName">RFC名称</param>
      /// <param name="lstParameters">参数列表</param>
      /// <param name="TableName">返回值Table结构名称</param>
      /// <returns></returns>
      public DataTable getSapFunctionToTable(string funName, Dictionary<string, string> lstParameters,
          string TableName)
      {
          DataTable dt = new DataTable();
          RfcRepository repo = prd.Repository;
          IRfcFunction function = repo.CreateFunction(funName);
          foreach (var item in lstParameters)
          {
              function.SetValue(item.Key, item.Value);
          }
          function.SetParameterActive(0, true);
          function.Invoke(prd);
          IRfcTable rtb = function.GetTable(TableName);
          dt = CreateTable(rtb, TableName);
          return dt;
      }
      #endregion

      #region 与SAP进行数据交互（MES->SAP）返回多个字符串+Table
      /// <summary>
      /// 
      /// </summary>
      /// <param name="funName">RFC名称</param>
      /// <param name="lstParameters">参数列表</param>
      /// <param name="TableName">返回值Table结构名称</param>
      /// <param name="outParameters">接收返回字符串List</param>
      /// <param name="outList">返回参数列表</param>
      /// <returns></returns>
      public DataTable getSapFunctionToTable(string funName, Dictionary<string, string> lstParameters,
          string TableName, out Dictionary<string, string> outParameters, List<string> outList)
      {
          Dictionary<string, string> outDataList = new Dictionary<string, string>();
          DataTable dt = new DataTable();
          RfcRepository repo = prd.Repository;
          IRfcFunction function = repo.CreateFunction(funName);
          foreach (var item in lstParameters)
          {
              function.SetValue(item.Key, item.Value);
          }
          function.SetParameterActive(0, true);
          function.Invoke(prd);
          IRfcTable rtb = function.GetTable(TableName);
          foreach (var item in outList)
          {
              outDataList[item] = function.GetString(item);
          }
          outParameters = outDataList;
          dt = CreateTable(rtb, TableName);
          return dt;
      }
      #endregion

      #region 根据返回的数据,生成DataTable
      /// <summary>
      /// 根据返回的数据,生成DataTable
      /// </summary>
      /// <param name="rtb"></param>
      /// <param name="TableName"></param>
      /// <returns></returns>
      private DataTable CreateTable(IRfcTable rtb, string TableName)
      {
          DataTable dt = new DataTable(TableName);

          //建立表结构，设定表头
          for (int i = 0; i <= rtb.ElementCount - 1; i++)
          {
              DataColumn col = new DataColumn(rtb.GetElementMetadata(i).Name);
              dt.Columns.Add(col);
          }
          //填充表数据  
          for (int k = 0; k <= rtb.RowCount - 1; k++)
          {
              DataRow dr = dt.NewRow();
              for (int i = 0; i <= rtb.ElementCount - 1; i++)
              {
                  dr[i] = rtb[k][i].GetValue();
              }
              dt.Rows.Add(dr);
          }
          return dt;
      }
      #endregion

      #region 定义要给SAP传送的数据列表(把实际的Table数据转换成SAP可识别的Table数据)
      /// <summary>
      /// 定义要给SAP传送的数据列表(把实际的Table数据转换成SAP可识别的Table数据)
      /// </summary>
      /// <param name="dt"></param>
      /// <param name="funName"></param>
      /// <param name="TableName"></param>
      /// <returns></returns>
      public IRfcTable CreateRfcTable(DataTable dt, string funName, string TableName)
      {
          RfcRepository repo = prd.Repository;
          IRfcFunction Z_RFC_ZCOX = repo.CreateFunction(funName);
          IRfcTable rtb = Z_RFC_ZCOX.GetTable(TableName, true);
          for (int i = 0; i < dt.Rows.Count; i++)
          {
              rtb.Append();
              foreach (DataColumn col in dt.Columns)
              {
                  rtb.SetValue(rtb[i][col.ColumnName].Metadata.Name, dt.Rows[i][col.ColumnName].ToString());
              }
          }
          return rtb;
      }
      #endregion

      #region 释放资源
      /// <summary>
      /// 释放资源
      /// </summary>
      public void Dispose()
      {
          RfcDestinationManager.UnregisterDestinationConfiguration(sapConfig);
      }
      #endregion
  }
2.7与SAP交互
	static void Main(string[] args)
  {
      Dictionary<string, string> keyValues = new Dictionary<string, string>();
      keyValues.Add("IM_BUDAT","2021-07-18");
      List<string> columns = new List<string>();
      SAPServerHelper sapHelper = new SAPServerHelper();
      var result =sapHelper.getSapFunctionToTable("ZOAFM_HWYD_INFO", keyValues, "OT_DATA");
      Console.WriteLine(result.Rows.Count);
      foreach(DataColumn item in result.Columns)
      {
          Console.WriteLine(item.ColumnName+"\t");
      }
      Console.WriteLine("Press any key outer.");
      Console.ReadKey();
  }
2.8 App.config配置项
	<appSettings>
    <!--Sap 服务器参数-->
    <add key="NAME" value="Aiko"/>
    <add key="ASHOST" value="172.16.101.207"/>
    <add key="CLIENT" value="800"/>
    <add key="USER" value="WMS03"/>
    <add key="PASSWD" value="Aiko12345"/>
    <add key="SYSNR" value="00"/>
    <add key="LANG" value="zh"/>
  </appSettings>
