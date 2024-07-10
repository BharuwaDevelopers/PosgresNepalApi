using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebServiceApp.Models
{
    public class OraDBConnection
    {
        static string Userid = "";
        static string password = "";
        static string datasorce = "";
        static string databasename = "";
        //public static void SetConnectiondata(string userid, string Pwd, string datasource)
        //{
        //    Userid = userid;
        //    password = Pwd;
        //    datasorce = datasource;
        //}

        public static void SetConnectiondata(string userid, string Pwd, string datasource, string dbName)
        {
            Userid = userid;
            password = Pwd;
            datasorce = datasource;
            databasename = dbName;

        }

        public static string OrclConnection
        {
            get
            {
                string _oraConn = null;
                try
                {
                     _oraConn = "Host=" + datasorce + ";Username = " + Userid + "; Password=" + password + ";Database=" + databasename + "";
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                return _oraConn;
            }
        }

        public static Npgsql.NpgsqlConnection connection(String connectionString)
        {
            Npgsql.NpgsqlConnection conn = null;
            using (conn = new Npgsql.NpgsqlConnection(connectionString))
            {
                conn.Open();
            }
            return conn;
        }
    }
}
