using System;
using Npgsql;

namespace ElefontNETStandard.Helpers
{
    /// <summary>
    /// Database connection to a npgsql db.
    /// </summary>
    public class DatabaseConnection : IDisposable
    {
        public readonly NpgsqlConnection Connection;

        private bool _isQuering;
        /// <summary>
        /// if true the connection is established and being used at the give moment.
        /// </summary>
        public bool IsQuering => _isQuering;
        public bool IsNotQuering => !_isQuering;

        protected bool KeepActive;

        public DatabaseConnection(string connString, bool keepActive)
        {
            Connection = new NpgsqlConnection(connString);
            KeepActive = keepActive;
        }

        /// <summary>
        /// The connection gets established.
        /// </summary>
        public void Post()
        {
            _isQuering = true;
            if (Connection.State != System.Data.ConnectionState.Open)
                Connection.Open();
        }

        /// <summary>
        /// The connection gets closed and the instance is suppressed if KeepActive flag is false.
        /// </summary>
        public void Dispose()
        {
            _isQuering = false;

            if (!KeepActive)
            {
                Connection.Close();
                GC.SuppressFinalize(this);
            }
        }
    }
}

