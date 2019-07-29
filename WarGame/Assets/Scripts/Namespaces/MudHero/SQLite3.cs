using System;
using System.IO;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace MudHero
{
    namespace SQLite3
    {
        public static class DBCommandUtility
        {
            public static string GetInsertCommand(string table, string[] ids, object[] values)
            {
                string tableIDs = string.Format("({0})",string.Join(",", ids));
                string tablesValues = string.Format("({0})", GetSQLiteVarField(values));
                return string.Format("INSERT OR IGNORE INTO {0} {1} VALUES {2}", table, tableIDs, tablesValues);
            }

            public static string GetUpdateCommand(string table, string searchKey, object searchValue, string[] keysToUpdate, object[] values)
            {
                string whereStatement = "";

                if (searchKey != null)
                {
                    if (searchKey.Length > 0)
                    {
                        if (searchValue.GetType() == typeof(string))
                            whereStatement = string.Format("WHERE {0} = \"{1}\"", searchKey, searchValue);
                        else if (searchValue.GetType() == typeof(int) || searchValue.GetType() == typeof(long))
                            whereStatement = string.Format("WHERE {0} = {1}", searchKey, searchValue);
                    }
                }

                return string.Format("UPDATE {0} SET {1} {2}", table, GetSQLiteEqualityField(keysToUpdate, values), whereStatement);
            }

            public static string GetSelectCommand(string table, string[] idsToGet)
            {
                string selectionIDs = string.Join(",", idsToGet);
                return string.Format("SELECT {0} FROM {1}", selectionIDs, table);
            }

            public static string GetConditionalSelectCommand(string table, string[] conditionalIDs, object[] conditionalValues, string[] idsToGet)
            {
                string whereStatement = "";

                if (conditionalIDs != null)
                    if (conditionalIDs.Length > 0)
                        whereStatement = string.Format("WHERE {0}", GetSQLiteEqualityField(conditionalIDs, conditionalValues));

                string selectionIDs = string.Join(",", idsToGet);
                return string.Format("SELECT {0} FROM {1} {2}", selectionIDs, table, whereStatement);
            }

            static string GetSQLiteVarField(object[] values)
            {
                string field = "";

                for (int v = 0; v < values.Length; v++)
                {
                    if (values[v] != null)
                    {
                        var type = values[v].GetType();

                        if (type == typeof(string))
                            field += string.Format("\"{0}\",", values[v]);
                        else if (type == typeof(long) || type == typeof(double))
                            field += values[v] + ",";
                    }
                    else
                    {
                        field += "NULL,";
                    }
                }

                field = field.Remove(field.Length - 1, 1);
                return field;
            }

            static string GetSQLiteEqualityField(string[] ids, object[] values)
            {
                string field = "";

                for(int v = 0; v < values.Length; v++)
                {
                    if (values[v] != null)
                    {
                        var type = values[v].GetType();

                        if (type == typeof(string))
                            field += string.Format("{0}=\"{1}\",", ids[v], values[v]);
                        else if (type == typeof(long) || type == typeof(double))
                            field += string.Format("{0}={1},", ids[v], values[v]);
                    }
                    else
                    {
                        field += string.Format("{0}=NULL,", ids[v]);
                    }
                }

                field = field.Remove(field.Length - 1, 1);

                return field;
            }
        }

        public class DBManager
        {
            string dbPath;
            string connection;
            IDbConnection dbConnection;
            IDataReader dbReader;
            IDbCommand createCommand;
            IDbCommand insertCommand;
            IDbCommand updateCommand;
            IDbCommand readerCommand;

            public DBManager(string databasePath, string dbName)
            {
                dbPath = databasePath;
                connection = string.Format("URI=file:{0}/{1}", dbPath, dbName);

                if (!Directory.Exists(dbPath))
                    Directory.CreateDirectory(databasePath);

                dbConnection = new SqliteConnection(connection);
            }

            public void Open()
            {
                dbConnection.Open();
            }

            public void Close()
            {
                dbConnection.Close();
            }

            public void CreateTable(string table, string tableDefinitions)
            {
                if (dbConnection.State == ConnectionState.Open)
                {
                    if (createCommand == null)
                        createCommand = dbConnection.CreateCommand();
                    try
                    {
                        createCommand.CommandText = string.Format("CREATE TABLE IF NOT EXISTS {0}({1})", table, tableDefinitions);
                        createCommand.ExecuteReader();
                    }
                    catch(Exception e)
                    {
                        Debug.Log(string.Format("Failed to Create Table. \n{0}", e.Message));
                    }
                }
            }

            public void Insert(string tableName, string[] tableIDs, object[] values)
            {
                if (dbConnection.State == ConnectionState.Open)
                {
                    if (insertCommand == null)
                        insertCommand = dbConnection.CreateCommand();
                    try
                    {
                        insertCommand.CommandText = DBCommandUtility.GetInsertCommand(tableName, tableIDs, values);
                        insertCommand.ExecuteNonQuery();
                    }
                    catch(Exception e)
                    {
                        Debug.Log(string.Format("Failed to Insert Row.\n{0}", e.Message));
                    }
                }
            }

            public void Update(string tableName, string searchKey, object searchValue, string[] keysToUpdate, object[] values)
            {
                if (dbConnection.State == ConnectionState.Open)
                {
                    if (updateCommand == null)
                        updateCommand = dbConnection.CreateCommand();

                    try
                    {
                        updateCommand.CommandText = DBCommandUtility.GetUpdateCommand(tableName, searchKey, searchValue, keysToUpdate, values);
                        updateCommand.ExecuteReader();
                    }
                    catch(Exception e)
                    {
                        Debug.Log(string.Format("Failed to Update Row.\n{0}",e.Message));
                    }
                }
            }
            
            public object[][] GetFromTable(string table, string[] conditionalIDs = null, object[] conditionalValues = null, string[] idsToGet = null)
            {
                List<object[]> rowObjects = new List<object[]>();

                if (dbConnection.State == ConnectionState.Open)
                {
                    if (readerCommand == null)
                        readerCommand = dbConnection.CreateCommand();

                    try
                    {
                        //Ifs are done this way because last string[] would mean all parameters are being used meaning we are doing conditional
                        if(idsToGet != null)
                            readerCommand.CommandText = DBCommandUtility.GetConditionalSelectCommand(table, conditionalIDs, conditionalValues, idsToGet);
                        else if(conditionalIDs != null) //ConditionalIDs become just regular ids to get.
                            readerCommand.CommandText = DBCommandUtility.GetSelectCommand(table, conditionalIDs);
                        else
                            readerCommand.CommandText = DBCommandUtility.GetSelectCommand(table, new string[] { "*" });

                        dbReader = readerCommand.ExecuteReader();

                        while (dbReader.Read())
                        {
                            object[] row = new object[dbReader.FieldCount];

                            for (int w = 0; w < row.Length; w++)
                            {
                                row[w] = dbReader.GetValue(w);
                            }

                            rowObjects.Add(row);
                        }

                        dbReader.Close();
                        return rowObjects.ToArray();
                    }
                    catch (Exception e)
                    {
                        dbReader.Close();
                        Debug.Log(string.Format("Failed to Get Objects.\n{0}", e.Message));
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
