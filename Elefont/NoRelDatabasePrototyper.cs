using System;
using Elefont.Helpers;

namespace Elefont
{
    internal class NoRelDatabasePrototyper : ConnectionPrototyper
    {
        public NoRelDatabasePrototyper(int max_conn, int max_act, string uri) : base(max_conn, max_act, uri)
        {
        }

        public NoRelDatabasePrototyper(int max_conn, int max_act, string host, string username, string password, string database, string port) : base(max_conn, max_act, host, username, password, database, port)
        {
        }

        private string GetTableName<T>() => $"NoRel{typeof(T).Name}";

        public void PostObject<T>(T obj, Func<T, object> indexSelector) => PostObjects(new List<T> { obj }, indexSelector);
        public void PostObjects<T>(IEnumerable<T> objects, Func<T, object> indexSelector)
        {
            if (objects?.Any() ?? false)
            {
                string table = GetTableName<T>();
                var csql = new CSQL(GetAvailableConnectionAsync())
                    .CREATE_TABLE(table, "id VARCHAR(255) PRIMARY KEY, json VARCHAR(255)", true);     

                foreach(var obj in objects)
                {
                    var id = indexSelector.Invoke(obj);
                    if(id is null)
                    {
                        throw new NullReferenceException("The index of an object cannot be null.");
                    }
                    else
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        csql.INSERT_INTO(table, "id, json")
                            .VALUES(id.ToString(), Newtonsoft.Json.JsonConvert.SerializeObject(obj)).SEMICOLON();
#pragma warning restore CS8604 // Possible null reference argument.
                    }
                }
                csql.Post();
            }
        }

        public IEnumerable<T>? GetObjects<T>()
        {
            string table = GetTableName<T>();
            List<T> objects = new List<T>();

            new CSQL(GetAvailableConnectionAsync())
                .SELECT("json")
                .FROM(table)
                .Post((query) =>
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    objects.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(query.GetString(0)));
#pragma warning restore CS8604 // Possible null reference argument.
                });
            return objects.Any() ? objects : null;
        }

        public IEnumerable<T>? GetObjects<T>(Func<T, bool> predicate)
        {
            return GetObjects<T>()?.Where(predicate);
        }

        public void UpdateObject<T>(object id, T obj)
        {
            string table = GetTableName<T>();
            new CSQL(GetAvailableConnectionAsync())
                .UPDATE(table, "id = {0}, json = {1}", id, Newtonsoft.Json.JsonConvert.SerializeObject(obj))
                .WHERE("id = {0}", id)
                .Post();
        }

        public void DeleteObject<T>(object id)
        {
            string table = GetTableName<T>();
            new CSQL(GetAvailableConnectionAsync())
                .DELETE_FROM(table)
                .WHERE("id = {0}", id)
                .Post();
        }
    }
}

