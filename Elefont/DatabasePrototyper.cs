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

        protected CSQL SELECT(string sql) => new CSQL(GetAvailableConnectionAsync().Result).SELECT(sql);
        protected CSQL UPDATE(string table, string sets, params object[] _params) => new CSQL(GetAvailableConnectionAsync().Result).UPDATE(table, sets, _params);
        protected CSQL INSERT_INTO(string table, string? fields = null) => new CSQL(GetAvailableConnectionAsync().Result).INSERT_INTO(table, fields);
        protected CSQL DELETE_FROM(string table) => new CSQL(GetAvailableConnectionAsync().Result).DELETE_FROM(table);
        protected CSQL DO() => new CSQL(GetAvailableConnectionAsync().Result).DO();
    }
}

