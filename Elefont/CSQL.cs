using System;
using Elefont.Exceptions;
using Elefont.Helpers;
using Elefont.Models;
using Npgsql;

namespace Elefont
{
    /// <summary>
    /// Class that helps formulating a query and posting it to a Npgsql connection.
    /// </summary>
    public class CSQL
    {
        public static readonly string UNI_DATEFORMAT = "yyyy-MM-dd";
        public static readonly string NULL_VALUE = "NULL";
        public static readonly string DEFAULT_VALUE = "default";
        private string _sql;
        protected string Sql
        {
            get => _sql;
            set => _sql = string.IsNullOrWhiteSpace(value) ? string.Empty : $"{value} ";
        }

        protected List<string> Statements;

        protected DatabaseConnection Connection;
        protected NpgsqlDataReader Reader;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CSQL()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Sql = string.Empty;
            Statements = new List<string>();
        }

        /// <summary>
        /// Format the given parameter to match the postgress sql.
        /// </summary>
        /// <param name="param">Parameter to rationalize</param>
        /// <param name="nullString">Substitute for null value</param>
        /// <returns>Rationalized parameter</returns>
        /// <exception cref="TypeNotFoundException">The parameter type is not recognized.</exception>
        protected string RationalizeParameter(object param, string nullString)
        {
            var type = param?.GetType();

            string wrap(string p) => $"'{p}'";

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8605 // Unboxing a possibly null value.
            if (type == typeof(Int32))
                return wrap(param.ToString());

            if (type == typeof(Double))
                return wrap(param.ToString().Replace(",", "."));

            if (type == typeof(string))
            {
                if (Statements.Contains(param.ToString()))
                {
                    return param.ToString();
                }
                return wrap(param.ToString().Trim().Replace("'", "''"));
            }

            if (type == typeof(bool))
                return wrap(param.ToString());

            if (type == typeof(DateTime))
                return wrap(((DateTime)param).ToString(UNI_DATEFORMAT));

            if (type == typeof(DateTimeOffset))
                return wrap(((DateTimeOffset)param).ToString());

            if (type == null)
                return nullString;
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8605 // Unboxing a possibly null value.

            throw new TypeNotFoundException($"Type {type} is not a supported type.");
        }

        /// <summary>
        /// Replaces parameter placeholders with the actual parameter values. Uses "default" if '?' is present before the placeholder, "null" if else.
        /// </summary>
        /// <param name="sql">Base sql to replace from</param>
        /// <param name="_params">Parameters for the substitution</param>
        /// <returns>New sql</returns>
        protected string ReplaceParameters(string sql, object[] _params)
        {
            string iteration_string, null_string;
            for (int i = 0; i < _params.Length; i++)
            {
                iteration_string = $"{{{i}}}";
                if (sql.Contains($"?{iteration_string}"))
                {
                    iteration_string = "?" + iteration_string;
                    null_string = DEFAULT_VALUE;
                }
                else
                {
                    null_string = NULL_VALUE;
                }

                sql = sql.Replace(iteration_string, RationalizeParameter(_params[i], null_string));
            }
            return sql;
        }

        /// <summary>
        /// Posts the query and keeps the connection opened.
        /// </summary>
        /// <param name="connection"></param>
        public void OpenConnection(DatabaseConnection connection)
        {
            Sql += ";";
            Connection = connection;
            Connection.Post();
            Reader = new NpgsqlCommand(Sql, connection.Connection).ExecuteReader();
        }

        /// <summary>
        /// Posts the query and closes the connection.
        /// </summary>
        /// <param name="connection"></param>
        public void Post(DatabaseConnection connection)
        {
            OpenConnection(connection);
            Close();
        }

        /// <summary>
        /// Posts the query, invokes the action and closes the connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="querySelector">Action with the query's response.</param>
        public void Post(DatabaseConnection connection, Action<CSQL> querySelector)
        {
            OpenConnection(connection);
            while (Read())
            {
                querySelector.Invoke(this);
            }
            Close();
        }

        /// <summary>
        /// Posts the query, invokes the function and closes the connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="querySelector">Function with the query's response, it should return an element for the list.</param>
        /// <returns>An IEnumerable containing each return on the the query response.</returns>
        public IEnumerable<T> Post<T>(DatabaseConnection connection, Func<CSQL, T> querySelector)
        {
            var objects = new List<T>();
            OpenConnection(connection);
            while (Read())
            {
                objects.Add(querySelector.Invoke(this));
            }
            Close();

            return objects;
        }

        public bool Read()
        {
            return Reader.Read();
        }

        internal async Task CloseAsync()
        {
            Connection.Dispose();
            await Reader.DisposeAsync();
            Reader.Close();
        }

        public void Close()
        {
            CloseAsync().Wait();
        }

        public bool IsDBNull(int index)
        {
            if (Reader.IsDBNull(index))
                return true;
            return false;
        }

        public int GetInt32(int index)
        {
            return Reader.GetInt32(index);
        }
        public int? GetNullableInt32(int index)
        {
            if (IsDBNull(index))
                return null;
            return GetInt32(index);
        }

        public double GetDouble(int index)
        {
            return Reader.GetDouble(index);
        }
        public double? GetNullableDouble(int index)
        {
            if (IsDBNull(index))
                return null;
            return GetDouble(index);
        }

        public string? GetString(int index)
        {
            if (IsDBNull(index))
                return null;
            return Reader.GetString(index);
        }

        public bool GetBoolean(int index)
        {
            return Reader.GetBoolean(index);
        }
        public bool? GetNullableBoolean(int index)
        {
            if (IsDBNull(index))
                return null;
            return GetBoolean(index);
        }

        public DateTime GetDateTime(int index)
        {
            if (IsDBNull(index))
                return new DateTime();
            return Reader.GetDateTime(index);
        }
        public DateTime? GetNullableDateTime(int index)
        {
            if (IsDBNull(index))
                return null;
            return Reader.GetDateTime(index);
        }

        internal CSQL CREATE_TABLE(string table, string body, bool ifNotExists = false)
        {
            Sql += $"CREATE TABLE {(ifNotExists ? "IF NOT EXISTS " : null)}{table}({body});";
            return this;
        }

        public CSQL DO()
        {
            Sql = "DO $$";
            return this;
        }

        public CSQL DECLARE(params string[] statements)
        {
            try
            {
                QueryParameterModel param;
                for (int i = 0; i < statements.Length; i++)
                {
                    param = new QueryParameterModel(i, statements[i]);
                    Sql += $"DECLARE {param.Parameter} {param.Type};";
                    Statements.Add(param.Parameter);
                }
            }
            catch(NullReferenceException ex)
            {
                throw new NullReferenceException($"{ex.Source}: A null statement has been declarated.", ex);
            }
            return this;
        }

        public CSQL RETURNING(string param, string statement)
        {
            Sql += $"RETURNING {param} INTO {statement}";
            return this;
        }

        public CSQL BEGIN()
        {
            Sql += "BEGIN";
            return this;
        }

        public CSQL END()
        {
            Sql += "END$$";
            return this;
        }

        protected CSQL JOIN_BASE(string join, string table, string condition)
        {
            Sql += $"{join} {table} ON ({condition})";
            return this;
        }

        public CSQL INNER_JOIN(string table, string condition) => JOIN_BASE("INNER JOIN", table, condition);

        public CSQL LEFT_JOIN(string table, string condition) => JOIN_BASE("LEFT JOIN", table, condition);

        public CSQL RIGHT_JOIN(string table, string condition) => JOIN_BASE("RIGHT JOIN", table, condition);

        public CSQL SELECT(string sql)
        {
            Sql += $"SELECT {sql}";
            return this;
        }

        public CSQL FROM(string sql)
        {
            Sql += $"FROM {sql}";
            return this;
        }

        public CSQL WHERE(string sql)
        {
            Sql += $"WHERE {sql}";
            return this;
        }

        public CSQL WHERE(string sql, params object[] _params)
        {
            Sql += $"WHERE {ReplaceParameters(sql, _params)}";
            return this;
        }

        public CSQL DELETE_FROM(string table)
        {
            Sql += $"DELETE FROM {table}";
            return this;
        }

        public CSQL UPDATE(string table, string sets, params object[] _params)
        {
            Sql += $"UPDATE {table} SET {ReplaceParameters(sets, _params)}";
            return this;
        }

        public CSQL INSERT_INTO(string table, string? fields = null)
        {
            Sql += $"INSERT INTO {table} ({fields}) ";
            return this;
        }

        /// <summary>
        /// Function that defined parameters to SET. Sets to "null" if the parameter is null.
        /// </summary>
        /// <param name="_params"></param>
        /// <returns></returns>
        public CSQL VALUES(params object[] _params)
        {
            Sql += "VALUES(";

            for (int i = 0; i < _params.Length; i++)
            {
                Sql += RationalizeParameter(_params[i], "NULL") + (i + 1 < _params.Length ? ", " : " ");
            }

            Sql += ") ";
            return this;
        }

        /// <summary>
        /// VALUES function with the option to decide wheter to parameters as default or null
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="_params"></param>
        /// <returns></returns>
        public CSQL DEFINED_VALUES(string sql, params object[] _params)
        {
            Sql += $"VALUES({ReplaceParameters(sql, _params)}) ";
            return this;
        }

        public CSQL ORDER_BY(string sql)
        {
            Sql += $"ORDER BY {sql}";
            return this;
        }

        public CSQL ON_CONFLICT(string sql)
        {
            Sql += $"ON CONFLICT ({sql}) DO";
            return this;
        }

        public CSQL NOTHING()
        {
            Sql += "NOTHING";
            return this;
        }

        public CSQL SEMICOLON()
        {
            Sql += ";";
            return this;
        }

        public CSQL COMMA()
        {
            Sql += ",";
            return this;
        }
    }
}