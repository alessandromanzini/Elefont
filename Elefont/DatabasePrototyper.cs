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

        protected CSQL SELECT(string sql) => new CSQL(GetAvailableConnection()).SELECT(sql);
        protected CSQL UPDATE(string table, string sets, params object[] _params) => new CSQL(GetAvailableConnection()).UPDATE(table, sets, _params);
        protected CSQL INSERT_INTO(string table, string? fields = null) => new CSQL(GetAvailableConnection()).INSERT_INTO(table, fields);
        protected CSQL DELETE_FROM(string table) => new CSQL(GetAvailableConnection()).DELETE_FROM(table);
        protected CSQL DO() => new CSQL(GetAvailableConnection()).DO();
    }
}

