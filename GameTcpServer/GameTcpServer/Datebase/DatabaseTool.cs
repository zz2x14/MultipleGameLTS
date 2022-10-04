using MySql.Data.MySqlClient;
using static DBContentLibrary;
public static class DatabaseTool
{
    private static MySqlBaseConnectionStringBuilder builder;
    private static MySqlConnection sqlConnection;
    private static MySqlCommand sqlCmd;
    private static string curDatabase;

    private const string DB_IP_SEVERT = "localhost";
    private const uint DB_PORT = 3306;
    private const string DB_USERID = "root";
    private const string DB_PASSWORD = "54winner";

    private const string SQ = "'";
    private const string SC = ";";

    public static void SelectDatabase(string database)
    {
        curDatabase = database;
        builder = new MySqlConnectionStringBuilder()
        {
            Server = DB_IP_SEVERT,
            Port = DB_PORT,
            UserID = DB_USERID,
            Password = DB_PASSWORD,
            Database = curDatabase
        };

        OpenDatabase();
    }
    private static void OpenDatabase()
    {
        try
        {
            sqlConnection = new MySqlConnection(builder.ConnectionString);
            sqlConnection.Open();
            sqlCmd = new MySqlCommand();
            sqlCmd.Connection = sqlConnection;
            Console.WriteLine("接入数据库成功" + sqlConnection.State);
        }
        catch (MySqlException e)
        {
            Console.WriteLine($"接入数据库时发生错误，错误码：{e.Code}，错误信息：{e.Message}");
            throw;
        }
    }

    public static void InsertInfo(string tableName,List<string> values,List<string> changeContent)
    {
        var insertStr = "";
        
        if (values.Count == 0 || changeContent.Count == 0 || values.Count != changeContent.Count)
        {
            Console.WriteLine("修改的目标数据或修改内容字符串长度不合法或两者长度对应");
            return;
        }

        insertStr = "insert into " + tableName + " (" + values[0] ;
        for (int i = 1; i < values.Count; i++)
        {
            insertStr += "," + values[i];
        }

        insertStr += ") value (" + string.Format(SQ + changeContent[0] + SQ);
        for (int i = 1; i < changeContent.Count; i++)
        {
            insertStr += "," + SQ +changeContent[i] + SQ;
        }

        insertStr += ");";

        sqlCmd.CommandText = insertStr;
        sqlCmd.ExecuteNonQuery();
    }

    //TODO:有外键约束的值不能直接修改 需要进行额外处理 现方便进度 暂取消UserID为约束
    public static void InsertRoleInfo(int userID, string roleName)
    {
        var insertStr =
            "insert into " + DBTABLE_ROLE + $"({DBLIST_USERID},{DBLIST_ROLENAME})" + " value " + "(" + userID + ","
            + SQ + roleName + SQ + ")" + SC;

        sqlCmd.CommandText = insertStr;
        sqlCmd.ExecuteNonQuery();
    }
    
    public static bool CheckSthUQDoExist(string value,string column_list,string tableName)
    {
        if (string.IsNullOrWhiteSpace(column_list) || string.IsNullOrWhiteSpace(tableName))
        {
            Console.WriteLine("传入查询的列名或表名为空");
            return false;
        }
        
        var selectStr = "select " + column_list + " from " + tableName + SC;

        sqlCmd.CommandText = selectStr;

        var reader = sqlCmd.ExecuteReader();

        while (reader.Read())
        {
            
            if (value != reader.GetString(column_list)) continue;
            reader.Close();
            return true;
        }
        
        reader.Close();
        return false;
    }

    public static (int usrID,MySqlDataReader reader) GetUserIDFromDB(string num, string pw)
    {
        var selectStr = "select " + DBLIST_USERID + " from " + DBTABLE_USER
                        + " where " + DBLIST_ANUM + " = " + SQ + num + SQ + " and " + DBLIST_PW + " = " + SQ + pw + SQ + SC;

        sqlCmd.CommandText = selectStr;

        var reader = sqlCmd.ExecuteReader();

        while (reader.Read())
        {
            return (reader.GetInt32(DBLIST_USERID),reader);
        }
        
        return (0, reader);
    }

    public static (int roleID,MySqlDataReader reader) GetRoleIDFromDB(string roleName)
    {
        var selectStr = "select " + DBLIST_ROLEID + " from " + DBTABLE_ROLE
                        + " where " + DBLIST_ROLENAME + " = " + SQ + roleName + SQ + SC;
        
        sqlCmd.CommandText = selectStr;

        var reader = sqlCmd.ExecuteReader();
        
        while (reader.Read())
        {
            return (reader.GetInt32(DBLIST_ROLEID),reader);
        }

        return (0, reader);
    }


    public static void CloseDataReader(MySqlDataReader reader)
    {
        reader.Close();
    }

    public static bool CheckUserPasswordCorrect(string num, string pw)
    {
        if (string.IsNullOrWhiteSpace(num) || string.IsNullOrWhiteSpace(pw))
        {
            Console.WriteLine("传入账号或密码为空");
            return false;
        }

        var selectStr = "select " + DBLIST_PW + " from "  + DBTABLE_USER + 
                        " where " + DBLIST_ANUM + " = " + SQ + num + SQ + SC ;

        sqlCmd.CommandText = selectStr;

        var reader = sqlCmd.ExecuteReader();

        while (reader.Read())
        {
            if (pw != reader.GetString(DBLIST_PW)) continue;
            reader.Close();
            return true;
        }

        reader.Close();
        return false;
    }
}