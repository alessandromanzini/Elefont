using System;
using Elefont.Helpers;

namespace Elefont
{
    public class DatabasePrototyper : ConnectionPrototyper
    {

        public DatabasePrototyper(int max_conn, int max_act, string uri) : base(max_conn, max_act, uri)
        {
        }

        public DatabasePrototyper(int max_conn, int max_act, string host, string username, string password, string database, string port) : base(max_conn, max_act, host, username, password, database, port)
        {
        }

        protected static CSQL SELECT(string sql) => new CSQL().SELECT(sql);
        protected static CSQL UPDATE(string table, string sets, params object[] _params) => new CSQL().UPDATE(table, sets, _params);
        protected static CSQL INSERT_INTO(string table, string? fields = null) => new CSQL().INSERT_INTO(table, fields);
        protected static CSQL DELETE_FROM(string table) => new CSQL().DELETE_FROM(table);
    }
}

