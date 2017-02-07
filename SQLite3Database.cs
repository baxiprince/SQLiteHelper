using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Windows.Forms;

namespace SQLite3Helper
{
    public class SQLite3Database
    {
        string dbConnection;


        //Default constructor
        public SQLite3Database()
        {
            dbConnection = "Data Source=default.db";
        }


        //Single paramter (string) constructor
        public SQLite3Database(string inputFile)
        {
            dbConnection = string.Format($"Data Source={inputFile}");
        }


        //Constructor for advanced connection options
        //  uses a dictionary with the options explicit
        public SQLite3Database(Dictionary<string, string> connectionOpts)
        {
            StringBuilder sbConn = new StringBuilder();

            foreach (KeyValuePair<string, string> row in connectionOpts)
            {
                sbConn.Append($"{row.Key}={row.Value}; ");
            }

            dbConnection = sbConn.ToString().Trim();

        }


        //Query database
        public DataTable QueryDB(string sqlQuery)
        {
            DataTable dt = new DataTable();

            try
            {
                SQLiteConnection cnn = new SQLiteConnection(dbConnection);
                cnn.Open();
                SQLiteCommand sqlCommand = new SQLiteCommand(cnn);
                sqlCommand.CommandText = sqlQuery;
                SQLiteDataReader reader = sqlCommand.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                cnn.Close();

            }
            catch (Exception exp)
            {
                throw new Exception(exp.Message);
            }

            return dt;
        }


        //Function other than query
        public int ExecuteNonQuery(string sql)
        {

            SQLiteConnection cnn = new SQLiteConnection(dbConnection);
            cnn.Open();
            SQLiteCommand sqlCommand = new SQLiteCommand(cnn);
            sqlCommand.CommandText = sql;
            int rowsupdated = sqlCommand.ExecuteNonQuery();
            cnn.Close();

            return rowsupdated;
        }


        //Retrieve single items from db
        public string ExecuteScalar(string sql)
        {

            SQLiteConnection cnn = new SQLiteConnection(dbConnection);
            cnn.Open();
            SQLiteCommand sqlCommand = new SQLiteCommand(cnn);
            sqlCommand.CommandText = sql;
            object value = sqlCommand.ExecuteScalar();
            cnn.Close();
            if (value != null)
            {
                return value.ToString();
            }

            return string.Empty;
        }


        //Update rows
        public Boolean Update(string tblName, Dictionary<string, string> data, string where)
        {
            StringBuilder sbVals = new StringBuilder();
            Boolean returncode = true;

            if (data.Count >= 1)
            {
                foreach (KeyValuePair<string, string> val in data)
                {
                    sbVals.Append(string.Format($" {val.Key.ToString()} = '{val.Value.ToString()}',"));
                }
                sbVals.Remove(sbVals.Length, 1);
            }

            try
            {
                this.ExecuteNonQuery(string.Format($"update {tblName} set {sbVals.ToString()} where {where};"));
            }
            catch
            {
                returncode = false;
            }

            return returncode;
        }


        //delete rows from DB
        public Boolean Delete(string tblName, string where)
        {
            Boolean returncode = true;

            try
            {
                this.ExecuteNonQuery(string.Format($"delete from {tblName} where {where};"));
            }
            catch (Exception fail)
            {
                MessageBox.Show(fail.Message);
                returncode = false;
            }

            return returncode;
        }


        //Insert into DB
        public Boolean Insert(string tblName, Dictionary<string, string> data)
        {
            Boolean returncode = true;

            StringBuilder sbColumns = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();

            string columns = string.Empty;
            string values = string.Empty;

            foreach (KeyValuePair<string, string> val in data)
            {
                sbColumns.Append(String.Format($" {val.Key.ToString()},"));
                sbValues.Append(String.Format($" '{val.Value}',"));
            }
            sbColumns.Remove(sbColumns.ToString().LastIndexOf(","), 1);
            sbValues.Remove(sbValues.ToString().LastIndexOf(","), 1);

            try
            {
                this.ExecuteNonQuery(
                    String.Format(
                        $"insert into {tblName}({sbColumns.ToString()}) values({sbValues.ToString()})"
                        )
                    );
            }
            catch (Exception fail)
            {
                MessageBox.Show(fail.Message);
                returncode = false;
            }

            return returncode;
        }


        //Clear Table data
        public Boolean ClearTable(string tblName)
        {
            Boolean returncode = true;

            try
            {
                this.ExecuteNonQuery(String.Format($"delete from {tblName};"));
            }
            catch
            {
                returncode = false;
            }

            return returncode;
        }


        //NUKE DB!!!
        public Boolean ClearDB()
        {
            Boolean returncode = true;

            DataTable dtTables;
            try
            {
                dtTables = this.QueryDB("select NAME from SQLITE_MASTER where type='table' order by NAME;");
                foreach (DataRow table in dtTables.Rows)
                {
                    this.ClearTable(table["NAME"].ToString());
                }

            }
            catch
            {
                returncode = false;
            }

            return returncode;
        }


    }
}
