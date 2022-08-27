using System;
namespace ElefontNETStandard
{
    public class DatabasePrototyper
    {
        protected int max_connections_count;
        protected int max_active_connections_count;
        protected string _connectionString;
        private DatabaseConnection[] _connections;
        protected DatabaseConnection Conn
        {
            get
            {
                foreach (var connection in _connections)
                    if (connection.IsNotQuering)
                        return connection;

                throw new Exception("All connections are being utilized.");
            }
        }

        public DatabasePrototyper(int max_conn, int max_act, string uri)
        {
            try
            {
                DecapsulateConnectionUri(uri);
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine($"{ex.Source}: Not accessible Environment Variable for conn uri.");
                throw;
            }

            Init(max_conn, max_act);
        }

        public DatabasePrototyper(int max_conn, int max_act, string host, string username, string password, string database, string port)
        {
            _connectionString = CreateConnectionString(host, username, password, database, port);

            Init(max_conn, max_act);
        }

        private static string CreateConnectionString(string host, string username, string password, string database, string port) => $"Host={host};Username={username};Password={password};Database={database};Port={port};";

        private void DecapsulateConnectionUri(string uri)
        {
            var conn_params = uri[11..].Split(':', '@', '/');
            _connectionString = CreateConnectionString(conn_params[2], conn_params[0], conn_params[1], conn_params[4], conn_params[3]);
        }

        private void Init(int max_conn, int max_act)
        {
            max_connections_count = max_conn;
            max_active_connections_count = max_act;

            _connections = new DatabaseConnection[max_connections_count];
            for (int i = 0; i < max_connections_count; i++)
                _connections[i] = new DatabaseConnection(_connectionString, i < max_active_connections_count);
        }

        protected static CSQL SELECT(string sql) => new CSQL().SELECT(sql);
        protected static CSQL UPDATE(string table, string sets, params object[] _params) => new CSQL().UPDATE(table, sets, _params);
        protected static CSQL INSERT_INTO(string table, string fields = null) => new CSQL().INSERT_INTO(table, fields);
        protected static CSQL DELETE_FROM(string table) => new CSQL().DELETE_FROM(table);
    }
}

