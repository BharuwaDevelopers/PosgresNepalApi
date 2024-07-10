using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebServiceApp.Models
{
    public class DataLayer
    {
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteDataset(connectionString, commandType, commandText, (OracleParameter[])null);
        }

        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //create & open a OracleConnection, and dispose of it after we are done.
            using (Npgsql.NpgsqlConnection cn = new Npgsql.NpgsqlConnection(connectionString))
            {
                cn.Open();

                //call the overload that takes a connection in place of the connection string
                return ExecuteDataset(cn.ToString(), commandType, commandText, commandParameters);
            }
        }

        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            //pass through the call providing null for the set of OracleParameters
            return ExecuteNonQuery(connectionString, commandType, commandText, (OracleParameter[])null);
        }

        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params OracleParameter[] commandParameters)
        {
            //create & open a OracleConnection, and dispose of it after we are done.
            using (Npgsql.NpgsqlConnection cn = new Npgsql.NpgsqlConnection(connectionString))
            {
                cn.Open();

                //call the overload that takes a connection in place of the connection string
                return ExecuteNonQuery(cn.ToString(), commandType, commandText, commandParameters);
            }
        }

    }
}
