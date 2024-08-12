using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using USBNet.Configuration;

namespace Utility.Database.MSSQL
{
    public class Base
    {
        private static string _sError;
        private static bool _bError;

        private static string _ConnectionString;

        public static string ConnectionString { get => _ConnectionString; set => _ConnectionString = value; }

        public static string getConnectionString()
        {
            USBNet.Configuration.Configuration _oConfig = new USBNet.Configuration.Configuration();

            string _sDefConn = _oConfig.getPropertyValue("DefaultConnection");

            ConnectionString = _oConfig.getConnectionString(_sDefConn);

            return ConnectionString;
        }

        public static object CreateConnection()
        {
            _bError = false;
            SqlConnection _oConn;

            try
            {
                _oConn = new SqlConnection(getConnectionString());
            }
            catch (SqlException sEx)
            {
                _sError = sEx.Message;
                _bError = true;
                return null;
            }

            return _oConn;
        }

        public static async Task<object> CreateConnectionAsync()
        {
            _bError = false;
            SqlConnection _oConn;

            try
            {
                string shold = getConnectionString();
                _oConn = new SqlConnection(getConnectionString());
                await _oConn.OpenAsync();
            }
            catch (SqlException sEx)
            {
                _sError = sEx.Message;
                _bError = true;
                return null;
            }

            return _oConn;
        }

        public static object CreateConnection(string sConnectionString)
        {
            _bError = false;
            SqlConnection _oConn;
            ConnectionString = sConnectionString;

            try
            {
                _oConn = new SqlConnection(sConnectionString);
            }
            catch (SqlException sEx)
            {
                _sError = sEx.Message;
                _bError = true;
                return null;
            }

            return _oConn;
        }

        public static async Task<object> CreateConnectionAsync(string sConnectionString)
        {
            _bError = false;
            SqlConnection _oConn;
            ConnectionString = sConnectionString;

            try
            {
                _oConn = new SqlConnection(sConnectionString);
                await _oConn.OpenAsync();
            }
            catch (SqlException sEx)
            {
                _sError = sEx.Message;
                _bError = true;
                return null;
            }

            return _oConn;
        }

        public static object ExecuteResultSet(SqlConnection oConn, string sSQL)
        {
            try
            {
                _bError = false;
                SqlDataAdapter _oAdaptor = new SqlDataAdapter();
                DataSet _oDataset = new DataSet();

                if (!isError())
                {
                    SqlCommand _oCmd = new SqlCommand(sSQL, oConn);
                    _oAdaptor.SelectCommand = _oCmd;
                    int iNumber = _oAdaptor.Fill(_oDataset);

                    if (iNumber > 0) return (object)_oDataset;
                }
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return null;
        }

        public static async Task<object> ExecuteResultSetAsync(SqlConnection oConn, string sSQL)
        {
            try
            {
                _bError = false;
                SqlDataAdapter _oAdaptor = new SqlDataAdapter();
                DataSet _oDataset = new DataSet();

                if (!isError())
                {
                    SqlCommand _oCmd = new SqlCommand(sSQL, oConn);
                    _oAdaptor.SelectCommand = _oCmd;
                    int iNumber = await Task.Run(() => _oAdaptor.Fill(_oDataset)); // Execute Fill asynchronously

                    if (iNumber > 0) return (object)_oDataset;
                }
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return null;
        }

        public static object ExecuteResultSet(string sConnectionString, string sSQL)
        {
            try
            {
                _bError = false;
                SqlConnection _oConn;
                SqlDataAdapter _oAdaptor = new SqlDataAdapter();
                DataSet _oDataset = new DataSet();

                ConnectionString = sConnectionString;
                _oConn = (SqlConnection)CreateConnection(sConnectionString);

                if (!isError())
                {
                    SqlCommand _oCmd = new SqlCommand(sSQL, _oConn);
                    _oAdaptor.SelectCommand = _oCmd;
                    int iNumber = _oAdaptor.Fill(_oDataset);

                    _oConn.Close();

                    if (iNumber > 0) return (object)_oDataset;
                }
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return null;
        }

        public static async Task<object> ExecuteResultSetAsync(string sConnectionString, string sSQL)
        {
            try
            {
                _bError = false;
                SqlConnection _oConn;
                SqlDataAdapter _oAdaptor = new SqlDataAdapter();
                DataSet _oDataset = new DataSet();

                ConnectionString = sConnectionString;
                _oConn = (SqlConnection)await CreateConnectionAsync(sConnectionString);

                if (!isError())
                {
                    SqlCommand _oCmd = new SqlCommand(sSQL, _oConn);
                    _oAdaptor.SelectCommand = _oCmd;
                    int iNumber = await Task.Run(() => _oAdaptor.Fill(_oDataset)); // Execute Fill asynchronously

                    _oConn.Close();

                    if (iNumber > 0) return (object)_oDataset;
                }
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return null;
        }

        public static object ExecuteResultSet(string sSQL)
        {
            try
            {
                _bError = false;
                SqlConnection _oConn;
                SqlDataAdapter _oAdaptor = new SqlDataAdapter();
                DataSet _oDataset = new DataSet();

                _oConn = (SqlConnection)CreateConnection();

                if (!isError())
                {
                    SqlCommand _oCmd = new SqlCommand(sSQL, _oConn);
                    _oAdaptor.SelectCommand = _oCmd;
                    int iNumber = _oAdaptor.Fill(_oDataset);

                    _oConn.Close();

                    if (iNumber > 0) return (object)_oDataset;
                }
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return null;
        }

        public static async Task<object> ExecuteResultSetAsync(string sSQL)
        {
            try
            {
                _bError = false;
                SqlConnection _oConn;
                SqlDataAdapter _oAdaptor = new SqlDataAdapter();
                DataSet _oDataset = new DataSet();

                _oConn = (SqlConnection)await CreateConnectionAsync();

                if (!isError())
                {
                    SqlCommand _oCmd = new SqlCommand(sSQL, _oConn);
                    _oAdaptor.SelectCommand = _oCmd;
                    int iNumber = await Task.Run(() => _oAdaptor.Fill(_oDataset)); // Execute Fill asynchronously

                    _oConn.Close();

                    if (iNumber > 0) return (object)_oDataset;
                }
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return null;
        }

        public static int ExecuteNonQuery(string sConnectionString, string sSQL)
        {
            ConnectionString = sConnectionString;

            try
            {
                _bError = false;
                SqlConnection _oConn;

                _oConn = (SqlConnection)CreateConnection(sConnectionString);

                SqlCommand _oCmd = new SqlCommand(sSQL, _oConn);

                return _oCmd.ExecuteNonQuery();
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return 0;
        }

        public static async Task<int> ExecuteNonQueryAsync(string sConnectionString, string sSQL)
        {
            ConnectionString = sConnectionString;

            try
            {
                _bError = false;
                SqlConnection _oConn;

                _oConn = (SqlConnection)await CreateConnectionAsync(sConnectionString);

                SqlCommand _oCmd = new SqlCommand(sSQL, _oConn);

                return await _oCmd.ExecuteNonQueryAsync();
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return 0;
        }

        public static int ExecuteCmd(SqlCommand oCommand)
        {
            try
            {
                return oCommand.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                return -1;
            }
        }

        public static async Task<int> ExecuteCmdAsync(SqlCommand oCommand)
        {
            try
            {
                return await oCommand.ExecuteNonQueryAsync();
            }
            catch (SqlException)
            {
                return -1;
            }
        }

        public static object ExecuteCmdwithResultSet(SqlCommand oCommand)
        {
            SqlConnection _oConn = oCommand.Connection;

            try
            {
                _bError = false;
                SqlDataAdapter _oAdaptor = new SqlDataAdapter();
                DataSet _oDataset = new DataSet();

                if (!isError())
                {
                    _oAdaptor.SelectCommand = oCommand;
                    int iNumber = _oAdaptor.Fill(_oDataset);

                    // Add output parameters to the end of the datatable
                    for (int iLoop = 0; iLoop < oCommand.Parameters.Count; iLoop++)
                    {
                        if (oCommand.Parameters[iLoop].Direction == ParameterDirection.Output)
                        {
                            DataColumn _oColumn = _oDataset.Tables[0].Columns.Add(oCommand.Parameters[iLoop].ParameterName);

                            for (int iLoop2 = 0; iLoop2 < _oDataset.Tables[0].Rows.Count; iLoop2++)
                            {
                                _oDataset.Tables[0].Rows[iLoop2].SetField<string>(_oColumn, oCommand.Parameters[iLoop].Value.ToString());
                            }
                        }
                    }

                    _oConn.Close();

                    if (iNumber > 0) return (object)_oDataset;
                }
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return null;
        }

        public static async Task<object> ExecuteCmdwithResultSetAsync(SqlCommand oCommand)
        {
            SqlConnection _oConn = oCommand.Connection;

            try
            {
                _bError = false;
                SqlDataAdapter _oAdaptor = new SqlDataAdapter();
                DataSet _oDataset = new DataSet();

                if (!isError())
                {
                    _oAdaptor.SelectCommand = oCommand;
                    int iNumber = await Task.Run(() => _oAdaptor.Fill(_oDataset)); // Execute Fill asynchronously

                    // Add output parameters to the end of the datatable
                    for (int iLoop = 0; iLoop < oCommand.Parameters.Count; iLoop++)
                    {
                        if (oCommand.Parameters[iLoop].Direction == ParameterDirection.Output)
                        {
                            DataColumn _oColumn = _oDataset.Tables[0].Columns.Add(oCommand.Parameters[iLoop].ParameterName);

                            for (int iLoop2 = 0; iLoop2 < _oDataset.Tables[0].Rows.Count; iLoop2++)
                            {
                                _oDataset.Tables[0].Rows[iLoop2].SetField<string>(_oColumn, oCommand.Parameters[iLoop].Value.ToString());
                            }
                        }
                    }

                    _oConn.Close();

                    if (iNumber > 0) return (object)_oDataset;
                }
            }
            catch (SqlException Ex)
            {
                _sError = Ex.Message;
                _bError = true;
            }

            return null;
        }

        public static string getError()
        {
            return _sError;
        }

        public static bool isError()
        {
            return _bError;
        }
    }
}
