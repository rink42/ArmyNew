using System.Data;
using System.Text;
using ArmyAPI.Commons;

/// <summary>
/// WriteLog 的摘要描述
/// </summary>
public class WriteLog : IDisposable
{
    #region 變數
    private bool _Disposed;

    private StringBuilder? _Msg = new StringBuilder();

    private string _OutputFilename = String.Format("{0:yyyyMMdd}.txt", DateTime.Today);

    private string _Key = DateTime.Now.ToString("HHmmssfffffff");

	private CustomService _CustomService;

    private static WriteLog? _Instance;

    #endregion 變數

    #region 屬性

    #region string OutputFilename
    public string OutputFilename
    {
        set
        {
            if (value.Length > 0)
                _OutputFilename = value;
            else
                _OutputFilename = String.Format("{0:yyyyMMdd}.txt", DateTime.Today);
        }
        get
        {
            return _OutputFilename;
        }
    }
    #endregion string OutputFilename

    #region string Key
    public string Key
    {
        set
        {
            _Key = value;
        }
        get
        {
            return _Key;
        }
    }
    #endregion string Key

    #endregion 屬性

    #region 建構子
    public WriteLog(CustomService customService)
    {
        //
        // TODO: 在此加入建構函式的程式碼
        //
        _CustomService = customService;
    }

    public WriteLog (CustomService customService, string key)
    {
        _CustomService = customService;
        _Key = key;
    }

    // 静态方法，初始化 WriteLog 实例
    public static void Initialize(CustomService customService)
    {
        _Instance = new WriteLog(customService);
    }

    #endregion 建構子

    #region 解構子
    ~WriteLog ()
    {
        Dispose(false);
    }
    #endregion 解構子

    #region 方法/靜態方法/私有方法

    #region Dispose()
    void IDisposable.Dispose ()
    {
        Dispose(false);
        GC.SuppressFinalize(this);
    }
    #endregion Dispose()

    #region Dispose(bool isDisposing)
    void Dispose (bool isDisposing)
    {
        if (!_Disposed)
        {
            if (_Msg != null)
            {
                Flush();
                _Msg = null;
            }

            _Disposed = true;
        }
    }
    #endregion Dispose(bool isDisposing)

    #region Close()
    public void Close ()
    {
        ((IDisposable)this).Dispose();
    }
	#endregion Close()

	#region void Output ()
	private void Output ()
    {
        if (_Msg != null && _Msg.Length > 0)
        {
            if (!_Msg.ToString().EndsWith(Environment.NewLine + Environment.NewLine))
                _Msg.Append(Environment.NewLine);

            try
            {
                string filename = Path.GetFileNameWithoutExtension(_OutputFilename);

                // Log 檔分段
                int i = 0;
                do
                {
                    //string fTmp = _CustomService.GetHtmlFilePath("Log\\" + filename + ((i != 0) ? "_" + i.ToString("D3") : String.Empty) + ".txt");
                    string fTmp = CoreHttpContext.MapPath("Log\\" + filename + ((i != 0) ? "_" + i.ToString("D3") : String.Empty) + ".txt");

                    if (File.Exists(fTmp))
                    {
                        FileInfo fi = new FileInfo(fTmp);
                        if (fi.Length > 1024 * 1024 * 2)
                        {
                            i++;
                            continue;
                        }
                        else
                        {
                            filename = fTmp;
                            break;
                        }
                    }
                    else
                    {
                        filename = fTmp;
                        break;
                    }
                } while (true);

				// 获取文件所在的目录路径
				string directoryPath = Path.GetDirectoryName(filename)!;

				// 检查目录是否存在，如果不存在则创建目录
				if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}

				using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine(String.Format("=={0}{1}===============", ((!System.Text.RegularExpressions.Regex.IsMatch(filename, "\\d{8}(_\\d{3})?\\.txt")) ? String.Format("{0:yyyyMMdd} ", DateTime.Today) : String.Empty), Key));
                        sw.Write(String.Format("{0}", _Msg.ToString()));
                    }
                }
            }
            catch
            {
            }
            finally
            {
                //Response.End (); 
            }

            _Msg.Length = 0;
        }
    }
    #endregion void Output ()

    #region void LogMsg (string msg)
    public void LogMsg (string msg)
    {
        LogMsg(String.Empty, msg);
    }
    #endregion void LogMsg (string msg)

    #region LogMsg<T>(T msg)
    public void LogMsg<T> (T msg)
    {
        LogMsg(String.Empty, msg);
    }
    #endregion LogMsg<T>(T msg)

    #region void LogMsg (string txt, string msg)
    public void LogMsg (string txt, string msg)
    {
        if (txt.Length > 0)
            _Msg!.AppendLine(String.Format("{0} = {1}", txt, msg));
        else
            _Msg!.AppendLine(msg);
    }
    #endregion void LogMsg (string txt, string msg)

    #region void LogMsg<T> (string txt, T msg)
    public void LogMsg<T> (string txt, T msg)
    {
        if (typeof(T).ToString().EndsWith("[]") || typeof(T).ToString().EndsWith("[System.String]"))
        {
            switch (typeof(T).ToString())
            {
                case "System.Byte[]":
                    for (int i = 0; i < ((byte[])Convert.ChangeType(msg!, typeof(byte[]))).Length; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((byte[])Convert.ChangeType(msg!, typeof(byte[])))[i]));
                    }
                    break;
                case "System.Int16[]":
                    for (int i = 0; i < ((short[])Convert.ChangeType(msg!, typeof(short[]))).Length; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((short[])Convert.ChangeType(msg!, typeof(short[])))[i]));
                    }
                    break;
                case "System.UInt16[]":
                    for (int i = 0; i < ((ushort[])Convert.ChangeType(msg!, typeof(ushort[]))).Length; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((ushort[])Convert.ChangeType(msg!, typeof(ushort[])))[i]));
                    }
                    break;
                case "System.Int32[]":
                    for (int i = 0; i < ((int[])Convert.ChangeType(msg!, typeof(int[]))).Length; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((int[])Convert.ChangeType(msg!, typeof(int[])))[i]));
                    }
                    break;
                case "System.UInt32[]":
                    for (int i = 0; i < ((uint[])Convert.ChangeType(msg!, typeof(uint[]))).Length; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((uint[])Convert.ChangeType(msg!, typeof(uint[])))[i]));
                    }
                    break;
                case "System.String[]":
                    for (int i = 0; i < ((string[])Convert.ChangeType(msg!, typeof(string[]))).Length; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((string[])Convert.ChangeType(msg!, typeof(string[])))[i]));
                    }
                    break;
                case "System.Single[]":
                    for (int i = 0; i < ((float[])Convert.ChangeType(msg!, typeof(float[]))).Length; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((float[])Convert.ChangeType(msg!, typeof(float[])))[i]));
                    }
                    break;
                case "System.Double[]":
                    for (int i = 0; i < ((double[])Convert.ChangeType(msg!, typeof(double[]))).Length; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((double[])Convert.ChangeType(msg!, typeof(double[])))[i]));
                    }
                    break;
                case "System.String[][]":
                    string[][] tmp = ((string[][])Convert.ChangeType(msg!, typeof(string[][])));
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        for (int j = 0; j < tmp[i].Length; j++)
                            _Msg!.AppendLine(String.Format("  msg[{0}][{1}] = {2}", i, j, tmp[i][j]));
                    }
                    break;
                case "System.Object[]":
                    for (int i = 0; i < ((object[])Convert.ChangeType(msg!, typeof(object[]))).Length; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((object[])Convert.ChangeType(msg!, typeof(object[])))[i]));
                    }
                    break;
                case "System.Collections.Generic.List`1[System.String]":
                    for (int i = 0; i < ((System.Collections.Generic.List<String>)Convert.ChangeType(msg!, typeof(System.Collections.Generic.List<String>))).Count; i++)
                    {
                        _Msg!.AppendLine(String.Format("  msg[{0}] = {1}", i, ((System.Collections.Generic.List<String>)Convert.ChangeType(msg!, typeof(System.Collections.Generic.List<String>)))[i]));
                    }
                    break;
            }

            if (txt.Length > 0)
                _Msg!.Insert(0, String.Format("{0} = " + Environment.NewLine, txt));
        }
        else
        {
            if (txt.Length > 0)
                _Msg!.AppendLine(String.Format("{0} = {1}", txt, msg));
            else
                _Msg!.AppendLine(msg!.ToString());
        }
    }
	#endregion void LogMsg<T> (string txt, T msg)

	#region void Flush ()
	public void Flush ()
    {
        Output();
        _Msg!.Length = 0;
    }
    #endregion void Flush ()

    #region 靜態方法

    #region static void CommonLog (string msg)
    private static void CommonLog (string msg)
    {
        CommonLog(String.Empty, msg);
    }
    #endregion static void CommonLog (string msg)

    #region static void CommonLog (string txt, string msg)
    private static void CommonLog (string txt, string msg)
    {
        if (txt.Length > 0)
            _Instance!.LogMsg(String.Format("{0} = {1}", txt, msg));
        else
            _Instance!.LogMsg(msg);
    }
    #endregion static void CommonLog (string txt, string msg)

    #region static void CommonLog<T> (T msg)
    private static void CommonLog<T> (T msg)
    {
        CommonLog(String.Empty, msg);
    }
    #endregion static void CommonLog<T> (T msg)

    #region static void CommonLog<T> (string txt, T msg)
    private static void CommonLog<T> (string txt, T msg)
    {
        if (txt.Length > 0)
            _Instance!.LogMsg<T>(txt, msg);
        else
            _Instance!.LogMsg<T>(msg);

        _Instance.Flush();
    }
    #endregion static void CommonLog<T> (string txt, T msg)

    #region static void CommonLog1 (string fileName, string msg)
    private static void CommonLog1 (string fileName, string msg)
    {
        CommonLog1(fileName, String.Empty, msg);
    }
    #endregion static void CommonLog1 (string fileName, string msg)

    #region static void CommonLog1 (string fileName, string txt, string msg)
    private static void CommonLog1 (string fileName, string txt, string msg)
    {
        _Instance!.OutputFilename = fileName;
        if (txt.Length > 0)
            _Instance.LogMsg(String.Format("{0} = {1}", txt, msg));
        else
            _Instance.LogMsg(msg);
    }
    #endregion static void CommonLog1 (string fileName, string txt, string msg)

    #region static void Log<T> (T msg)
    public static void Log<T> (T msg)
    {
        CommonLog(msg);
    }
    #endregion static void Log<T> (T msg)

    #region static void Log<T> (string txt, T msg)
    public static void Log<T> (string txt, T msg)
    {
        if (msg != null)
        {
            CommonLog(txt, msg);
        }
        else
        {
            CommonLog(String.Format("{0} = NULL！", txt));
        }
    }
    #endregion static void Log<T> (string txt, T msg)

    #region static void Log (string txt, int msg)
    public static void Log (string txt, int msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, int msg)

    #region static void Log (string txt, uint msg)
    public static void Log (string txt, uint msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, uint msg)

    #region static void Log (string txt, byte msg)
    public static void Log (string txt, byte msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, byte msg)

    #region static void Log (string txt, short msg)
    public static void Log (string txt, short msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, short msg)

    #region static void Log (string txt, ushort msg)
    public static void Log (string txt, ushort msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, ushort msg)

    #region static void Log (string txt, float msg)
    public static void Log (string txt, float msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, float msg)

    #region static void Log (string txt, double msg)
    public static void Log (string txt, double msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, double msg)

    #region static void Log (string txt, long msg)
    public static void Log (string txt, long msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, long msg)

    #region static void Log (string txt, ulong msg)
    public static void Log (string txt, ulong msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, ulong msg)

    #region static void Log (string txt, string msg)
    public static void Log (string txt, string msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, string msg)

    #region static void Log (string txt, string[] msg)
    public static void Log (string txt, string[] msg)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < msg.Length; i++)
            sb.AppendLine(String.Format("{0}[{1}] = {2}", txt, i, msg[i]));

        CommonLog(sb.ToString());
    }
    #endregion static void Log (string txt, string[] msg)

    #region static void Log (string txt, DateTime msg)
    public static void Log (string txt, DateTime msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg));
    }
    #endregion static void Log (string txt, DateTime msg)

    #region static void Log (string txt, Exception msg)
    public static void Log (string txt, Exception msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg.ToString()));
    }
    #endregion static void Log (string txt, Exception msg)

    #region static void Log (string txt, System.Data.SqlClient.SqlException msg)
    public static void Log (string txt, System.Data.SqlClient.SqlException msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg.ToString()));
    }
    #endregion static void Log (string txt, System.Data.SqlClient.SqlException msg)

    #region static void Log (string txt, System.Text.StringBuilder msg)
    public static void Log (string txt, System.Text.StringBuilder msg)
    {
        CommonLog(String.Format("{0} = {1}", txt, msg.ToString()));
    }
    #endregion static void Log (string txt, System.Text.StringBuilder msg)


    #region static void Log1<T> (string filename, string msg)
    public static void Log1<T> (string filename, string msg)
    {
        CommonLog1(filename, msg);
    }
    #endregion static void Log1<T> (string filename, string msg)

    #region static void Log1<T> (string fileName, string txt, T msg)
    public static void Log1<T> (string fileName, string txt, T msg)
    {
        if (msg != null)
            Log1<T>(fileName, txt + " = " + msg.ToString());
        else
            Log1<T>(fileName, txt + " IS NULL ！");
    }
    #endregion static void Log1<T> (string fileName, string txt, T msg)

    #region static void Log1<T> (string fileName, string txt, T msg, bool isNewLine)
    public static void Log1<T> (string fileName, string txt, T msg, bool isNewLine)
    {
        Log1<String>(fileName, txt + " = " + msg!.ToString() + Environment.NewLine);
    }
    #endregion static void Log1<T> (string fileName, string txt, T msg, bool isNewLine)

    #region static void Log1 (string fileName, string txt, int msg)
    public static void Log1 (string fileName, string txt, int msg)
    {
        Log1<int>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, int msg)

    #region static void Log1 (string fileName, string txt, uint msg)
    public static void Log1 (string fileName, string txt, uint msg)
    {
        Log1<uint>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, uint msg)

    #region static void Log1 (string fileName, string txt, byte msg)
    public static void Log1 (string fileName, string txt, byte msg)
    {
        Log1<byte>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, byte msg)

    #region static void Log1 (string fileName, string txt, short msg)
    public static void Log1 (string fileName, string txt, short msg)
    {
        Log1<short>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, short msg)

    #region static void Log1 (string fileName, string txt, ushort msg)
    public static void Log1 (string fileName, string txt, ushort msg)
    {
        Log1<ushort>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, ushort msg)

    #region static void Log1 (string fileName, string txt, float msg)
    public static void Log1 (string fileName, string txt, float msg)
    {
        Log1<float>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, float msg)

    #region static void Log1 (string fileName, string txt, double msg)
    public static void Log1 (string fileName, string txt, double msg)
    {
        Log1<double>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, double msg)

    #region static void Log1 (string fileName, string txt, long msg)
    public static void Log1 (string fileName, string txt, long msg)
    {
        Log1<long>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, long msg)

    #region static void Log1 (string fileName, string txt, ulong msg)
    public static void Log1 (string fileName, string txt, ulong msg)
    {
        Log1<ulong>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, ulong msg)

    #region static void Log1 (string fileName, string txt, string msg)
    public static void Log1 (string fileName, string txt, string msg)
    {
        Log1<string>(fileName, txt, msg);
    }
    #endregion static void Log1 (string fileName, string txt, string msg)

    #region static void Log1 (string fileName, string txt, string[] msg)
    public static void Log1 (string fileName, string txt, string[] msg)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < msg.Length; i++)
            sb.AppendLine(String.Format("{0}[{1}] = {2}", txt, i, msg[i]));

        Log1<string>(fileName, sb.ToString());
    }
    #endregion static void Log1 (string fileName, string txt, string[] msg)

    #region static void Log1 (string fileName, string txt, DateTime msg)
    public static void Log1 (string fileName, string txt, DateTime msg)
    {
        Log1<DateTime>(fileName, txt, msg);
    }
    #endregion static void Log1<T> (string fileName, string txt, DateTime msg)

    #region static void Log1 (string fileName, string txt, Exception msg)
    public static void Log1 (string fileName, string txt, Exception msg)
    {
        Log1<Exception>(fileName, txt, msg);
    }
    #endregion static void Log1<T> (string fileName, string txt, Exception msg)

    #region static void Log1 (string fileName, string txt, System.Data.SqlClient.SqlException msg)
    public static void Log1 (string fileName, string txt, System.Data.SqlClient.SqlException msg)
    {
        Log1<System.Data.SqlClient.SqlException>(fileName, txt, msg);
    }
    #endregion static void Log1<T> (string fileName, string txt, System.Data.SqlClient.SqlException msg)

    #region static void Log1 (string fileName, string txt, System.Text.StringBuilder msg)
    public static void Log1 (string fileName, string txt, System.Text.StringBuilder msg)
    {
        Log1<System.Text.StringBuilder>(fileName, txt, msg);
    }
    #endregion static void Log1<T> (string fileName, string txt, System.Text.StringBuilder msg)

    #region static void LogDataView (ref DataView dv)
    public static void LogDataView (ref DataView dv)
    {
        if (dv != null)
        {
            DataTable dt = dv.ToTable();
            LogDataTable(ref dt);
        }
        else
            Log<String>("[LogDataView (ref DataView dv)] dv is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataView (ref DataView dv)

    #region static void LogDataView (String filename, ref DataView dv)
    public static void LogDataView (String filename, ref DataView dv)
    {
        if (dv != null)
        {
            DataTable dt = dv.ToTable();
            LogDataTable(filename, ref dt);
        }
        else
            Log1<String>(filename, "[LogDataView (ref DataView dv)] dv is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataView (String filename, ref DataView dv)

    #region static void LogDataTable(ref DataTable dt)
    public static void LogDataTable (ref DataTable dt)
    {
        LogDataTable(ref dt, -1);
    }
    #endregion static void LogDataTable(ref DataTable dt)

    #region static void LogDataTable(ref DataTable dt, int logCount)
    public static void LogDataTable (ref DataTable dt, int logCount)
    {
        if (dt != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (logCount != -1 && i == logCount)
                        break;

                    for (int k = 0; k < dt.Columns.Count; k++)
                    {
                        string columnName = dt.Columns[k].ColumnName;
                        sb.AppendLine(String.Format("{0} = {1}", columnName, dt.Rows[i][columnName].ToString()));
                    }
                    sb.AppendLine(String.Empty);
                }
            }
            else
            {
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    string columnName = dt.Columns[k].ColumnName;
                    sb.AppendLine(String.Format("dt.Columns[{0}] = {1}", k, columnName));
                    sb.AppendLine(String.Empty);
                }
            }

            Log<String>(sb.ToString());
        }
        else
            Log<String>("[LogDataTable (ref DataTable dt, int logCount)] dt is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataTable(ref DataTable dt, int logCount)

    #region static void LogDataSet(ref DataSet ds)
    public static void LogDataSet (ref DataSet ds)
    {
        if (ds != null)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                Log<String>(String.Format("Table[{0}]：{1}", i + 1, ds.Tables[i].TableName));
                DataTable dt = ds.Tables[i];
                LogDataTable(ref dt);
            }
        }
        else
            Log<String>("[LogDataSet (ref DataSet ds)] ds is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataSet(ref DataSet ds)

    #region static void LogDataSet(ref DataSet ds, int logCount)
    public static void LogDataSet (ref DataSet ds, int logCount)
    {
        if (ds != null)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                Log<String>(String.Format("Table[{0}]：{1}", i + 1, ds.Tables[i].TableName));
                DataTable dt = ds.Tables[i];
                LogDataTable(ref dt, logCount);
            }
        }
        else
            Log<String>("[LogDataSet (ref DataSet ds, int logCount)] ds is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataSet(ref DataSet ds, int logCount)

    #region static void LogDataView (DataView dv)
    public static void LogDataView (DataView dv)
    {
        if (dv != null)
            LogDataTable(dv.ToTable());
        else
            Log<String>("[LogDataView (DataView dv)] dv is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataView (DataView dv)

    #region static void LogDataTable(DataTable dt)
    public static void LogDataTable (DataTable dt)
    {
        LogDataTable(dt, -1);
    }
    #endregion static void LogDataTable(DataTable dt)

    #region static void LogDataTable(DataTable dt, int logCount)
    public static void LogDataTable (DataTable dt, int logCount)
    {
        if (dt != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (logCount != -1 && i == logCount)
                        break;

                    for (int k = 0; k < dt.Columns.Count; k++)
                    {
                        string columnName = dt.Columns[k].ColumnName;
                        sb.AppendLine(String.Format("{0} = {1}", columnName, dt.Rows[i][columnName]));
                    }
                    sb.AppendLine(String.Empty);
                }
            }
            else
            {
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    string columnName = dt.Columns[k].ColumnName;
                    sb.AppendLine(String.Format("dt.Columns[{0}] = {1}", k, columnName));
                    sb.AppendLine(String.Empty);
                }
            }

            Log<String>(sb.ToString());
        }
        else
            Log<String>("[LogDataTable (DataTable dt, int logCount)] dt is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataTable(DataTable dt, int logCount)

    #region static void LogDataSet(DataSet ds)
    public static void LogDataSet (DataSet ds)
    {
        if (ds != null)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                Log<String>(String.Format("Table[{0}]：{1}", i + 1, ds.Tables[i].TableName));

                LogDataTable(ds.Tables[i]);
            }
        }
        else
            Log<String>("[LogDataSet (DataSet ds)] ds is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataSet(DataSet ds)

    #region static void LogDataSet(DataSet ds, int logCount)
    public static void LogDataSet (DataSet ds, int logCount)
    {
        if (ds != null)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                Log<String>(String.Format("Table[{0}]：{1}", i + 1, ds.Tables[i].TableName));

                LogDataTable(ds.Tables[i], logCount);
            }
        }
        else
            Log<String>("[LogDataSet (DataSet ds, int logCount)] ds is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataSet(DataSet ds, int logCount)


    #region static void LogDataTable(String filename, ref DataTable dt)
    public static void LogDataTable (String filename, ref DataTable dt)
    {
        LogDataTable(filename, ref dt, -1);
    }
    #endregion static void LogDataTable(String filename, ref DataTable dt)

    #region static void LogDataTable(String filename, ref DataTable dt, int logCount)
    public static void LogDataTable (String filename, ref DataTable dt, int logCount)
    {
        if (dt != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (logCount != -1 && i == logCount)
                        break;

                    for (int k = 0; k < dt.Columns.Count; k++)
                    {
                        string columnName = dt.Columns[k].ColumnName;
                        sb.AppendLine(String.Format("{0} = {1}", columnName, dt.Rows[i][columnName].ToString()));
                    }
                    sb.AppendLine(String.Empty);
                }
            }
            else
            {
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    string columnName = dt.Columns[k].ColumnName;
                    sb.AppendLine(String.Format("dt.Columns[{0}] = {1}", k, columnName));
                    sb.AppendLine(String.Empty);
                }
            }

            Log1<String>(filename, sb.ToString());
        }
        else
            Log1<String>(filename, "[LogDataTable (ref DataTable dt, int logCount)] dt is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataTable(String filename, ref DataTable dt, int logCount)

    #region static void LogDataSet(String filename, ref DataSet ds)
    public static void LogDataSet (String filename, ref DataSet ds)
    {
        if (ds != null)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                Log1<String>(filename, String.Format("Table：{0}", i + 1));
                DataTable dt = ds.Tables[i];
                LogDataTable(filename, ref dt);
            }
        }
        else
            Log1<String>(filename, "[LogDataSet (ref DataSet ds)] ds is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataSet(String filename, ref DataSet ds)

    #region static void LogDataSet(String filename, ref DataSet ds, int logCount)
    public static void LogDataSet (String filename, ref DataSet ds, int logCount)
    {
        if (ds != null)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                Log1<String>(filename, String.Format("Table：{0}", i + 1));
                DataTable dt = ds.Tables[i];
                LogDataTable(filename, ref dt, logCount);
            }
        }
        else
            Log1<String>(filename, "[LogDataSet (ref DataSet ds, int logCount)] ds is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataSet(String filename, ref DataSet ds, int logCount)

    #region static void LogDataView (String filename, DataView dv)
    public static void LogDataView (String filename, DataView dv)
    {
        if (dv != null)
            LogDataTable(filename, dv.ToTable());
        else
            Log1<String>(filename, "[LogDataView (DataView dv)] dv is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataView (String filename, DataView dv)

    #region static void LogDataTable(String filename, DataTable dt)
    public static void LogDataTable (String filename, DataTable dt)
    {
        LogDataTable(filename, dt, -1);
    }
    #endregion static void LogDataTable(String filename, DataTable dt)

    #region static void LogDataTable(String filename, DataTable dt, int logCount)
    public static void LogDataTable (String filename, DataTable dt, int logCount)
    {
        if (dt != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (logCount != -1 && i == logCount)
                        break;

                    for (int k = 0; k < dt.Columns.Count; k++)
                    {
                        string columnName = dt.Columns[k].ColumnName;
                        sb.AppendLine(String.Format("{0} = {1}", columnName, dt.Rows[i][columnName]));
                    }
                    sb.AppendLine(String.Empty);
                }
            }
            else
            {
                for (int k = 0; k < dt.Columns.Count; k++)
                {
                    string columnName = dt.Columns[k].ColumnName;
                    sb.AppendLine(String.Format("dt.Columns[{0}] = {1}", k, columnName));
                    sb.AppendLine(String.Empty);
                }
            }

            Log1<String>(filename, sb.ToString());
        }
        else
            Log1<String>(filename, "[LogDataTable (DataTable dt, int logCount)] dt is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataTable(String filename, DataTable dt, int logCount)

    #region static void LogDataSet(String filename, DataSet ds)
    public static void LogDataSet (String filename, DataSet ds)
    {
        if (ds != null)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                Log1<String>(filename, String.Format("Table：{0}", i + 1));

                LogDataTable(filename, ds.Tables[i]);
            }
        }
        else
            Log1<String>(filename, "[LogDataSet (DataSet ds)] ds is NULL！" + Environment.NewLine);
    }
    #endregion static void LogDataSet(String filename, DataSet ds)

    #region LogDataSet(String filename, DataSet ds, int logCount)
    public static void LogDataSet (String filename, DataSet ds, int logCount)
    {
        if (ds != null)
        {
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                Log1<String>(filename, String.Format("Table：{0}", i + 1));

                LogDataTable(filename, ds.Tables[i], logCount);
            }
        }
        else
            Log1<String>(filename, "[LogDataSet (DataSet ds, int logCount)] ds is NULL！" + Environment.NewLine);
    }
    #endregion LogDataSet(String filename, DataSet ds, int logCount)

    #endregion 靜態方法

    #endregion 方法/靜態方法/私有方法
}
