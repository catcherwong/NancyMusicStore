using Dapper;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace NancyMusicStore.Common
{
    public class DBHelper
    {
        //open connection       
        private static IDbConnection OpenConnection()
        {
            var conn = new NpgsqlConnection(ConfigHelper.GetConneectionStr());
            conn.Open();
            return conn;
        }

        //execute 
        public static int Execute(string sql, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var conn = OpenConnection())
            {
                return conn.Execute(sql, param, transaction, commandTimeout, commandType);
            }
        }

        //execute 
        public static object ExecuteScalar(string cmd, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null)
        {
            using (var conn = OpenConnection())
            {
                return conn.ExecuteScalar(cmd, param, transaction, commandTimeout, commandType);
            }
        }

        //do query and return a list
        public static IList<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null,
            bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            using (var conn = OpenConnection())
            {
                return conn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType).ToList();
            }
        }

        //do query and return the first entity
        public static T QueryFirstOrDefault<T>(string sql, object param = null, IDbTransaction transaction = null,
            int? commandTimeout = null, CommandType? commandType = null) where T : class
        {
            using (var conn = OpenConnection())
            {
                return conn.QueryFirstOrDefault<T>(sql, param, transaction, commandTimeout, commandType);
            }
        }
    }
}