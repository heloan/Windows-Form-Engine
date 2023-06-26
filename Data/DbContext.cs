using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Univap.Programacao3.Data
{
    public class DbContext
    {
        public static MySqlConnection Context { get; private set; }
        public static bool IsConnect { get; private set; }

        public DbContext()
        {
            if (!IsConnect)
                throw new Exception("Data base not connect!");
        }

        public DbContext(string server, string database, string uid, string password)
        {
            try
            {
                Context = new MySqlConnection($"server={server};database={database};uid={uid};password={password};");
                Context.Open();
                IsConnect = true;
            }
            catch (Exception ex)
            {
                Context = null;
                throw (ex);
            }
        }

        public static List<T> GetAll<T>() where T : new()
        {
            try
            {
                List<T> table = new List<T>();
                string nomeDaTabela = typeof(T).Name;

                MySqlCommand commandSql = Context.CreateCommand();
                commandSql.CommandText = $"Select * from {nomeDaTabela}";

                using (MySqlDataReader respostaConsulta = commandSql.ExecuteReader())
                {
                    DataTable schemaTable = respostaConsulta.GetSchemaTable();
                    int columnCount = schemaTable.Rows.Count;

                    while (respostaConsulta.Read())
                    {
                        T row = new T();
                        for (int i = 0; i < columnCount; i++)
                        {
                            PropertyInfo pinfo = typeof(T).GetProperty(schemaTable.Rows[i]["ColumnName"].ToString());
                            pinfo.SetValue(row, Convert.ChangeType(respostaConsulta.GetValue(i), pinfo.PropertyType), null);
                        }
                        table.Add(row);
                    }

                }

                return table;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static List<T> GetAll<T>(String propName, String propValue) where T : new()
        {
            try
            {
                List<T> table = new List<T>();
                string nomeDaTabela = typeof(T).Name;
                string command = $"{propName}='{propValue}'";

                MySqlCommand commandSql = Context.CreateCommand();
                commandSql.CommandText = $"Select * from {nomeDaTabela} where {command}";

                using (MySqlDataReader respostaConsulta = commandSql.ExecuteReader())
                {
                    DataTable schemaTable = respostaConsulta.GetSchemaTable();
                    int columnCount = schemaTable.Rows.Count;

                    while (respostaConsulta.Read())
                    {
                        T row = new T();
                        for (int i = 0; i < columnCount; i++)
                        {
                            PropertyInfo pinfo = typeof(T).GetProperty(schemaTable.Rows[i]["ColumnName"].ToString());
                            pinfo.SetValue(row, Convert.ChangeType(respostaConsulta.GetValue(i), pinfo.PropertyType), null);
                        }
                        table.Add(row);
                    }

                }

                return table;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static T Get<T>(T row)
        {
            try
            {
                string nomeDaTabela = typeof(T).Name;
                string primaryKey = GetPrimaryKey(nomeDaTabela);
                var prop = typeof(T).GetProperty(primaryKey);
                string command = $"{prop.Name}='{prop.GetValue(row, null).ToString()}'";

                MySqlCommand commandSql = Context.CreateCommand();
                commandSql.CommandText = $"Select * from {nomeDaTabela} where {command}";

                using (MySqlDataReader respostaConsulta = commandSql.ExecuteReader())
                {
                    DataTable schemaTable = respostaConsulta.GetSchemaTable();
                    int columnCount = schemaTable.Rows.Count;

                    while (respostaConsulta.Read())
                    {
                        for (int i = 0; i < columnCount; i++)
                        {
                            PropertyInfo pinfo = typeof(T).GetProperty(schemaTable.Rows[i]["ColumnName"].ToString());

                            if (pinfo != null)
                            {
                                pinfo.SetValue(row, Convert.ChangeType(respostaConsulta.GetValue(i), pinfo.PropertyType), null);
                            }
                        }
                    }

                }

                return row;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static T Get<T>(T row, String propName, String propValue)
        {
            try
            {
                string nomeDaTabela = typeof(T).Name;
                string command = $"{propName}='{propValue}'";

                MySqlCommand commandSql = Context.CreateCommand();
                commandSql.CommandText = $"Select * from {nomeDaTabela} where {command}";

                using (MySqlDataReader respostaConsulta = commandSql.ExecuteReader())
                {
                    DataTable schemaTable = respostaConsulta.GetSchemaTable();
                    int columnCount = schemaTable.Rows.Count;

                    while (respostaConsulta.Read())
                    {
                        for (int i = 0; i < columnCount; i++)
                        {
                            PropertyInfo pinfo = typeof(T).GetProperty(schemaTable.Rows[i]["ColumnName"].ToString());

                            if (pinfo != null)
                            {
                                pinfo.SetValue(row, Convert.ChangeType(respostaConsulta.GetValue(i), pinfo.PropertyType), null);
                            }
                        }
                    }

                }

                return row;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static int Insert<T>(T row)
        {
            try
            {
                string nomeDaTabela = typeof(T).Name;
                string command = String.Empty;
                List<string> propList = new List<string>();
                List<string> valuesList = new List<string>();
                string autoIncrement = DbContext.GetAutoIncrement(nomeDaTabela);

                foreach (var prop in row.GetType().GetProperties())
                {
                    if (prop.Name != autoIncrement && prop.GetValue(row, null) != null)
                    {
                        propList.Add(prop.Name);

                        if (prop.PropertyType == typeof(DateTime))
                            valuesList.Add($"'{((DateTime)prop.GetValue(row, null)).ToString("yyyy-MM-dd HH:mm:ss")}'");
                        else
                            valuesList.Add($"'{prop.GetValue(row, null).ToString()}'");
                    }
                }

                MySqlCommand commandSql = Context.CreateCommand();
                commandSql.CommandText = $"insert into {nomeDaTabela} ({String.Join(",", propList)}) " +
                                            $"values ({String.Join(",", valuesList)})";

                for (int i = 0; i < propList.Count; i++)
                {
                    if (valuesList[i] == "'False'" || valuesList[i] == "'True'")
                    {
                        int convertResult = valuesList[i] == "'True'" ? 1 : 0;
                        commandSql.Parameters.AddWithValue(propList[i], convertResult) ;
                    }   
                    else
                        commandSql.Parameters.AddWithValue(propList[i], valuesList[i]);
                }

                commandSql.Prepare();
                var result = commandSql.ExecuteNonQuery();

                if (!String.IsNullOrEmpty(autoIncrement))
                {
                    PropertyInfo pinfo = typeof(T).GetProperty(autoIncrement);
                    pinfo.SetValue(row, Convert.ChangeType(commandSql.LastInsertedId, pinfo.PropertyType), null);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static int Update<T>(T row)
        {
            try
            {
                string nomeDaTabela = typeof(T).Name;
                List<string> command = new List<string>();
                List<string> propList = new List<string>();
                List<string> valuesList = new List<string>();

                string primaryKey = GetPrimaryKey(nomeDaTabela);

                foreach (var prop in row.GetType().GetProperties())
                {
                    if (prop.GetValue(row, null) != null)
                    {
                        propList.Add(prop.Name);
                        valuesList.Add($"'{prop.GetValue(row, null).ToString()}'");
                        command.Add($"{prop.Name}='{prop.GetValue(row, null).ToString()}'");
                    }
                }

                MySqlCommand commandSql = Context.CreateCommand();
                commandSql.CommandText = $"update {nomeDaTabela} set {String.Join(", ", command)} " +
                                            $"where {command.FirstOrDefault(y => y.Contains(primaryKey))}";

                for (int i = 0; i < propList.Count; i++)
                    commandSql.Parameters.AddWithValue(propList[i], valuesList[i]);

                commandSql.Prepare();

                return commandSql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw (ex);
            }


        }

        public static int Delete<T>(T row)
        {
            try
            {
                string nomeDaTabela = typeof(T).Name;
                List<string> command = new List<string>();

                string primaryKey = GetPrimaryKey(nomeDaTabela);
                string valueKey = row.GetType().GetProperty(primaryKey).GetValue(row).ToString();

                MySqlCommand commandSql = Context.CreateCommand();
                commandSql.CommandText = $"delete from {nomeDaTabela} where {primaryKey}='{valueKey}'";

                commandSql.Parameters.AddWithValue(primaryKey, valueKey);
                commandSql.Prepare();

                return commandSql.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw (ex);
            }


        }

        private static string GetPrimaryKey(string tabela)
        {
            var query = $"SHOW KEYS FROM {tabela} WHERE Key_name = 'PRIMARY'";
            using (var command = new MySqlCommand(query, Context))
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var columnName = reader.GetString("Column_name");
                    return columnName;
                }
                else
                {
                    throw new Exception($"Não foi possível encontrar a chave primária da tabela '{tabela}'");
                }
            }
        }

        private static string GetAutoIncrement(string tabela)
        {
            var query = $"SHOW COLUMNS FROM {tabela} WHERE Extra = 'auto_increment'";
            using (var command = new MySqlCommand(query, Context))
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read()) // Check if reader contains any rows
                    return reader.GetString("Field");
                else
                    return null;
            }
        }
    }
}
