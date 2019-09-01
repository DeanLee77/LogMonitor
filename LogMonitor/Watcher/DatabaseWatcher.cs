using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMonitor.Watcher
{
    public class DatabaseWatcher : BaseWatcher
    {
        private string _dataSource;
        private string _databaseName;

        private string _connectionString;
        private SqlConnection _sqlConnection;
        private SqlCommand _sqlCommand;
        private SqlDependency _sqlDependency;


        public DatabaseWatcher(string dataSource, string databaseName, string userId)
            : base()
        {
            _databaseName = databaseName;
            _dataSource = dataSource;

            // this is local db => Data Source = (localdb)\ProjectsV13; databaseName => Portal
            _connectionString = $"Data Source={dataSource};Initial Catalog={databaseName};user id={userId};Integrated Security=True;ApplicationIntent = ReadWrite; MultiSubnetFailover = False; MultipleActiveResultSets = true ";
            _sqlConnection = new SqlConnection(_connectionString);
            _sqlConnection.Open();

            EnablingServiceBroker();
            SqlDependency.Start(_connectionString);
        }

        public override void RunThread()
        {
            _sqlCommand = _sqlConnection.CreateCommand();
            _sqlCommand.CommandText = $"SELECT DocumentID, Name, Submitted, Error, DateCreated, DateCreatedBy, DateModified, DateModifiedBy FROM dbo.Documents ORDER BY DocumentID DESC;";
            _sqlDependency = new SqlDependency(_sqlCommand);

            _sqlDependency.OnChange += new OnChangeEventHandler(OnDataChange);
        
            string scalar =_sqlCommand.ExecuteScalar().ToString();
            this.Wait();

        }

        private void OnDataChange(object sender, SqlNotificationEventArgs e)
        {
            SqlDependency sqlDependency = sender as SqlDependency;
            sqlDependency.OnChange -= new OnChangeEventHandler(OnDataChange);

            Console.WriteLine($"Database change deteced----Chagne Type: {e.Type}, Info: {e.Info} ");
            _sqlCommand.Notification = null;
            sqlDependency = new SqlDependency(_sqlCommand);
            sqlDependency.OnChange += new OnChangeEventHandler(OnDataChange);
            string scalar = _sqlCommand.ExecuteScalar().ToString();
            int recentInsertId;
            if (!int.TryParse(scalar, out recentInsertId))
            {
                Console.WriteLine("Recent inserted ID cannot be found!");
            }
            
            ReadWriteData(recentInsertId);
            //Console.WriteLine($"scalar: {scalar}");

        }

        private void ReadWriteData(int recentInsertDocumentId)
        {
            
            SqlCommand command = _sqlConnection.CreateCommand();
            command.CommandText = $"SELECT DocumentID, Name, Submitted, Error, DateCreated, DateCreatedBy, DateModified, DateModifiedBy FROM dbo.Documents WHERE DocumentID = {recentInsertDocumentId};";

            using (var reader = command.ExecuteReader())
            {
                string name, outPut;
                string error = null, dateCreatedBy = null, dateModifiedBy = null ;
                int submitted;
                DateTime? dateCreated = null, dateModified= null;

                while (reader.Read())
                {
                    name = reader.GetString(reader.GetOrdinal("Name"));
                    submitted = reader.GetInt32(reader.GetOrdinal("Submitted"));
                    if(!reader.IsDBNull(reader.GetOrdinal("Error")))
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

                    WriteOnFile(new MasterLogger(DateTime.Now.ToString("dd/MM/yy hh:mm tt"), _dataSource + "/" + _databaseName, error, outPut));
                }
                reader.Close();
            }
        }

        private void WriteOnFile(MasterLogger m_logger)
        {

            Object locker = new Object();
            lock (locker)
            {
                using (StreamWriter sw = File.AppendText(_consolidatedLogFilePath))
                {
                    sw.Write(JsonConvert.SerializeObject(m_logger));
                    sw.Write("\n");
                    sw.Flush();
                    sw.Close();
                    Console.WriteLine("Finished having read database and written it onto a consolidated log file");
                }
            }
        }

        private string checkingServiceBroker()
        {
            using (SqlCommand sqlCommand = _sqlConnection.CreateCommand())
            {
                sqlCommand.CommandText = $"SELECT is_broker_enabled FROM sys.databases WHERE name = '{_databaseName}';";
                //select is_broker_enabled from sys.databases where name = 'Portal';
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader["is_broker_enabled"].ToString();
                    }
                }
                return "";
            }
        }
        private  void EnablingServiceBroker()
        {
            if(checkingServiceBroker()  == "False")
            {
                using (SqlCommand sqlCommand = _sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = $"ALTER DATABASE {_databaseName} SET ENABLE_BROKER with rollback immediate;";
                    sqlCommand.ExecuteNonQuery();
                }
            }
            
        }

        public override void Finish()
        {
            SqlDependency.Stop(_connectionString);
            _sqlConnection.Close();
            this.SetSignal();
            this.Join();
        }

        private SqlConnection GetSqlConnection()
        {
            return _sqlConnection;
        }
        
    }
}
