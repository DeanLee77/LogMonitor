using LogMonitor.Watcher;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LogFileWatcherTest
{
    [TestClass]
    public class DatabaseWatcherTest
    {
        private int _watchId = 1;
        private string _dataSource = @"DESKTOP-64JOJJP\SQLEXPRESS";
        private string _databaseName = "Portal";
        private string _userId = "dean";

        public DatabaseWatcher ArrangeDatabaseWatcher()
        {
            DatabaseWatcher dataWatcher = new DatabaseWatcher(_watchId, _dataSource, _databaseName, _userId);
            dataWatcher.Start();

            return dataWatcher;
        }

        public SqlConnection CreateSqlConnection()
        {
            //create a new sqlConnection to a same database as the databaseWatcher watches for changes, and insert a new row to the datbase
            string connectionString = $"Data Source={_dataSource};Initial Catalog={_databaseName};user id={_userId};Integrated Security=True;ApplicationIntent = ReadWrite; MultiSubnetFailover = False; MultipleActiveResultSets = true ";
            SqlConnection newSqlConnection = new SqlConnection(connectionString);
            newSqlConnection.Open();

            return newSqlConnection;

        }

        public void CloseSqlConnection(SqlConnection sqlConnection)
        {
            sqlConnection.Close();
        }

        public string GetInsertedDocumentID(SqlConnection sqlConnection)
        {

            SqlCommand command = sqlConnection.CreateCommand();

            int documentId = -1;

            command.CommandText = $"INSERT INTO Documents(Name, Data, Submitted, DateCreated, DateCreatedBy, DateModified, DateModifiedBy) VALUES('TEST Information 12', 'ADAFE&^768742dfkajfiepad', 2, '2019-07-12 10:26:38', 'haha', '2019-07-13 11:26:38', 'haha');";
            command.ExecuteNonQuery();

            command.CommandText = @"SELECT MAX(DocumentID) AS DocumentID FROM documents";
            SqlDataReader newResult = command.ExecuteReader();
            while (newResult.Read())
            {
                documentId = newResult.GetInt32(newResult.GetOrdinal("DocumentID"));
            }
            newResult.Close();

            return documentId.ToString();
        }

        public MasterLogger CreateMasterLogger(SqlConnection sqlConnection, string recentInsertDocumentId)
        {
            SqlCommand command = sqlConnection.CreateCommand();
            command.CommandText = $"SELECT * FROM dbo.Documents WHERE DocumentID = {recentInsertDocumentId};";

            SqlDataReader reader = command.ExecuteReader();

            string name, outPut = null;
            string error = null, dateCreatedBy = null, dateModifiedBy = null;
            int submitted;
            DateTime? dateCreated = null, dateModified = null;

            while (reader.Read())
            {
                name = reader.GetString(reader.GetOrdinal("Name"));
                submitted = reader.GetInt32(reader.GetOrdinal("Submitted"));
                if (!reader.IsDBNull(reader.GetOrdinal("Error")))
                {
                    error = reader.GetString(reader.GetOrdinal("Error"));
                }
                if (!reader.IsDBNull(reader.GetOrdinal("DateCreated")))
                {
                    dateCreated = reader.GetDateTime(reader.GetOrdinal("DateCreated"));
                }
                if (!reader.IsDBNull(reader.GetOrdinal("DateCreatedBy")))
                {
                    dateCreatedBy = reader.GetString(reader.GetOrdinal("DateCreatedBy"));
                }
                if (!reader.IsDBNull(reader.GetOrdinal("DateModified")))
                {
                    dateModified = reader.GetDateTime(reader.GetOrdinal("DateModified"));
                }
                if (!reader.IsDBNull(reader.GetOrdinal("DateModifiedBy")))
                {
                    dateModifiedBy = reader.GetString(reader.GetOrdinal("DateModifiedBy"));
                }

                outPut = $"Document Name: {name}, Submitted: {submitted}, Date created: {dateCreated}, Date created by: {dateCreatedBy}, Date modified: {dateModified}, Date modified by: {dateModifiedBy}";

                return new MasterLogger(DateTime.Now.ToString("dd/MM/yy hh:mm tt"), _dataSource + "/" + _databaseName, error, outPut);
            }
            return new MasterLogger(DateTime.Now.ToString("dd/MM/yy hh:mm tt"), _dataSource + "/" + _databaseName, error, outPut);
        }

        [TestMethod]
        public void DataWatcherTest()
        {

            //start a new databaseWatcher
            DatabaseWatcher dataWatcher = ArrangeDatabaseWatcher();


            //expect serviceBroker: on
            bool serviceBrokerOn = false;

            SqlConnection sqlConnection = CreateSqlConnection();
            SqlCommand sqlCommand = sqlConnection.CreateCommand();
            sqlCommand.CommandText = $"SELECT is_broker_enabled FROM sys.databases WHERE name = '{_databaseName}';";
            using (var reader = sqlCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["is_broker_enabled"].ToString() == "True")
                    {
                        serviceBrokerOn = true;
                    }
                }
            }

            string newDocumentId = GetInsertedDocumentID(sqlConnection); //insert a new value and get its id from the database
            MasterLogger m_logger = CreateMasterLogger(sqlConnection, newDocumentId);

            string consolidatedLogFilePath = @"C:\Windows\Temp\consolidated log\consolidatedLog.log";

            Thread.Sleep(4000);
            FileStream fs = new FileStream(consolidatedLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sReader = new StreamReader(fs, Encoding.UTF8);
            string line;
            string lastLine = "";
            while((line = sReader.ReadLine()) != null)
            {
                lastLine = line;
            }

            string expectedJsonString = JsonConvert.SerializeObject(m_logger);
            bool newValueIsOnFile = lastLine.Equals(expectedJsonString);

            CloseSqlConnection(sqlConnection);

            bool hasAllExpectedValue = serviceBrokerOn && newValueIsOnFile ? true : false;
            dataWatcher.Finish();
            Assert.AreEqual(true, hasAllExpectedValue);
        }


        

        
    }
}
