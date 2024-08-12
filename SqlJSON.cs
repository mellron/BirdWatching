using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Utility.Database.MSSQL.JSON
{
    public class SqlJSON
    {
        // Synchronous method
        public static string ExecuteResultSet(string sConnectionString, string sSQL)
        {
            string _JSON = GetJSONString((DataSet)Base.ExecuteResultSet(sConnectionString, sSQL));

            if (Base.isError())
                return null;
            else
                return _JSON;
        }

        // Asynchronous method
        public static async Task<string> ExecuteResultSetAsync(string sConnectionString, string sSQL)
        {
            string _JSON = GetJSONString((DataSet)await Base.ExecuteResultSetAsync(sConnectionString, sSQL));

            if (Base.isError())
                return null;
            else
                return _JSON;
        }

        // Synchronous method
        public static string ExecuteCmdwithResultSet(SqlCommand oCommand)
        {
            string _JSON = GetJSONString((DataSet)Base.ExecuteCmdwithResultSet(oCommand));

            if (Base.isError())
                return null;
            else
                return _JSON;
        }

        // Asynchronous method
        public static async Task<string> ExecuteCmdwithResultSetAsync(SqlCommand oCommand)
        {
            string _JSON = GetJSONString((DataSet)await Base.ExecuteCmdwithResultSetAsync(oCommand));

            if (Base.isError())
                return null;
            else
                return _JSON;
        }

        public static DataSet ConvertJSONToDataSet(string JSON)
        {
            try
            {
                return JsonConvert.DeserializeObject<DataSet>(JSON);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetJSONOutputString(DataSet Dt, bool bBackWardComp = true)
        {
            string _sJSON = GetJSONString(Dt);

            if (bBackWardComp)
            {
                _sJSON = _sJSON[1..];
                _sJSON = "{\"Records\" : [" + _sJSON + "}";
            }

            return _sJSON;
        }

        public static string GetJSONString(DataSet Dt)
        {
            return GetJSONString(Dt.Tables[0]);
        }

        public static string GetJSONString(DataTable Dt)
        {
            if (Dt != null && Dt.Rows.Count > 0)
            {
                string json = JsonConvert.SerializeObject(Dt, Formatting.None, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                return json;
            }
            return null;
        }
    }
}
