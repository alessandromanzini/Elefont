using System;
using Elefont.Exceptions;

namespace Elefont.Helpers
{
    public abstract class ConnectionPrototyper
    {
        protected int max_connections_count;
        protected int max_active_connections_count;

        protected string _connectionString;
        private DatabaseConnection[] _connections;
        protected async Task<DatabaseConnection> GetAvailableConnectionAsync()
        {
            const int max_attempts = 5;
            int attempts_count = 0;
            while(attempts_count <= max_attempts)
            {
                foreach(var conn in _connections)
                    if (conn.IsNotQuering)
                        return conn;
                await Task.Delay(1000);
            }
            throw new ConnectionException("Max connections count reached.");
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ConnectionPrototyper(int max_conn, int max_act, string uri)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            DecapsulateConnectionUri(uri);

            Init(max_conn, max_act);
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ConnectionPrototyper(int max_conn, int max_act, string host, string username, string password, string database, string port)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _connectionString = CreateConnectionString(host, username, password, database, port);

            Init(max_conn, max_act);
        }

        private static string CreateConnectionString(string host, string username, string password, string database, string port) => $"Host={host};Username={username};Password={password};Database={database};Port={port};";

        private void DecapsulateConnectionUri(string uri)
        {
            try
            {
                var conn_params = uri[11..].Split(':', '@', '/');
                _connectionString = CreateConnectionString(conn_params[2], conn_params[0], conn_params[1], conn_params[4], conn_params[3]);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException($"{ex.Source}: The specified connection uri is null.", ex);
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException($"{ex.Source}: The specified connection uri is missing information.", ex);
            }
        }

        private void Init(int max_conn, int max_act)
        {
            max_connections_count = max_conn;
            max_active_connections_count = max_act;

            _connections = new DatabaseConnection[max_connections_count];
            for (int i = 0; i < max_connections_count; i++)
                _connections[i] = new DatabaseConnection(_connectionString, i < max_active_connections_count);
        }
    }
}

