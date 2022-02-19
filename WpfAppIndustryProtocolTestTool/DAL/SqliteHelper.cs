using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Threading.Tasks;

namespace WpfAppIndustryProtocolTestTool.DAL
{
    public class SqliteHelper: IDatabaseHelper
    {
        #region Singleton Pattern

        static SqliteHelper? _instance;
        static readonly object _lockObject = new object();
        SqliteHelper() { }

        public static SqliteHelper GetSqliteHelpeInstance()
        {
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new SqliteHelper();
                    }
                }
            }
            return _instance;
        }

        #endregion

        SqliteConnectionStringBuilder connnectionCfg = new SqliteConnectionStringBuilder
        {
            DataSource = @"./Asset/Database/SqliteDB",
            Mode = SqliteOpenMode.ReadWriteCreate
        };

        #region CommandText

        const string tblSerialPortInfo = @"CREATE TABLE serial_port_information ( 
                                               port_id    INTEGER PRIMARY KEY ASC ON CONFLICT ABORT AUTOINCREMENT
                                                                  NOT NULL ON CONFLICT ABORT,
                                               port_name  TEXT    NOT NULL,
                                               baud_rate  TEXT    NOT NULL,
                                               parity     TEXT    NOT NULL,
                                               data_bits  TEXT    NOT NULL,
                                               stop_bits  TEXT    NOT NULL, 
                                               hand_shake TEXT    NOT NULL 
                                           );";

        const string tblSerialPortMsg = @"CREATE TABLE serial_port_message ( 
                                              message_id   INTEGER PRIMARY KEY ASC AUTOINCREMENT
                                                                   NOT NULL,
                                              port_id      INTEGER NOT NULL
                                                                   REFERENCES serial_port_information ( port_id ) MATCH FULL,
                                              send_receive TEXT    NOT NULL,
                                              content      TEXT,
                                              time_stamp   TEXT    NOT NULL 
                                          );";

        const string tblInfoMsg = @"CREATE TABLE info_message ( 
                                        info_id              INTEGER PRIMARY KEY ASC AUTOINCREMENT
                                                                     NOT NULL,
                                        info_level           TEXT    NOT NULL,
                                        info_source          TEXT    NOT NULL,
                                        info_source_id       TEXT    NOT NULL,
                                        info_content         TEXT    NOT NULL,
                                        info_time_stamp      TEXT    NOT NULL 
                                    );";

        const string tblEthernetPortInfo = @"CREATE TABLE ethernet_connection_information ( 
                                                 connection_id       INTEGER PRIMARY KEY ASC ON CONFLICT ABORT AUTOINCREMENT
                                                                             NOT NULL ON CONFLICT ABORT,
                                                 work_role           TEXT    NOT NULL,
                                                 ipv4_address        TEXT    NOT NULL,
                                                 port                TEXT    NOT NULL,
                                                 maximum_clients     TEXT    NOT NULL,
                                                 receive_buffer_size TEXT    NOT NULL
                                             );";

        const string tblEthernetPortMsg = @"CREATE TABLE ethernet_port_message ( 
                                                 message_id       INTEGER PRIMARY KEY ASC ON CONFLICT ABORT AUTOINCREMENT
                                                                          NOT NULL,
                                                 connection_id    INTEGER NOT NULL
                                                                          REFERENCES ethernet_connection_information ( connection_id ) MATCH FULL,
                                                 content          TEXT,
                                                 send_receive     TEXT    NOT NULL,
                                                 remote_endpoint  TEXT    NOT NULL,
                                                 time_stamp       TEXT    NOT NULL 
                                             );";

        const string triggerSerialPortMsg = @"CREATE TRIGGER trigger_serial_Msg AFTER INSERT ON serial_port_message 
                                                      BEGIN
                                                            INSERT INTO serial_port_message(time_stamp) VALUES(datetime('now'));
                                                      END;";

        const string triggerEthernetPortMsg = @"CREATE TRIGGER trigger_ethernet_Msg AFTER INSERT ON ethernet_port_message 
                                                      BEGIN
                                                            INSERT INTO ethernet_port_message(time_stamp) VALUES(datetime('now'));
                                                      END;";

        #endregion

        #region Private Methods

        private Task<SqliteCommand> PrepareCommandAsync(SqliteConnection connection, string commandText, params SqliteParameter[] parameters)
        {
            return Task.Run(() =>
             {
                 if (connection.State != ConnectionState.Open)
                 {
                     connection.Open();
                 }
                 var command = connection.CreateCommand();
                 command.CommandText = commandText;
                 command.CommandType = CommandType.Text;
                 command.CommandTimeout = 10;
                 command.Parameters.Clear();

                 if (parameters != null)
                 {
                     foreach (var para in parameters)
                     {
                         command.Parameters.AddWithValue(para.ParameterName, para.Value);
                     }
                 }
                 return command;
             });

        }

        private async void CreateTableAsync(SqliteConnection connection, string tblName, string cmdText)
        {
            try
            {
                string commandText = @"SELECT * FROM sqlite_master WHERE type='table' AND name=$tableName ";
                SqliteParameter paraTableName = new SqliteParameter("$tableName", tblName);
                var command = await PrepareCommandAsync(connection, commandText, paraTableName);

                bool result;
                using (var reader = command.ExecuteReader())
                {
                    result = reader.HasRows;
                }
                if (!result)
                {
                    command = await PrepareCommandAsync(connection, cmdText);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void CreateTriggerAsync(SqliteConnection connection, string triggerName, string cmdText)
        {
            try
            {
                string commandText = @"SELECT * FROM sqlite_master WHERE type='trigger' AND name=$triggerName ";
                SqliteParameter paraName = new SqliteParameter("$triggerName", triggerName);
                var command = await PrepareCommandAsync(connection, commandText, paraName);

                bool result;
                using (var reader = command.ExecuteReader())
                {
                    result = reader.HasRows;
                }
                if (!result)
                {
                    command = await PrepareCommandAsync(connection, cmdText);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        private Task<DataTable> FillDataAsync(SqliteCommand command)
        {
            return Task.Run(() =>
            {
                DataTable dataTable = new DataTable();
                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            int fieldCount = reader.FieldCount;
                            for (int i = 0; i < fieldCount; i++)
                            {
                                dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                            }

                            dataTable.BeginLoadData();
                            object[] values = new object[fieldCount];
                            while (reader.Read())
                            {
                                reader.GetValues(values);
                                dataTable.LoadDataRow(values, true);
                            }
                        }
                    }
                    dataTable.EndLoadData();
                    return dataTable;
                }
                catch (Exception)
                {
                    throw;
                }
            });

        }

        #endregion

        public void InitializeSqliteDB()
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                CreateTableAsync(connection, "serial_port_information", tblSerialPortInfo);
                CreateTableAsync(connection, "serial_port_message", tblSerialPortMsg);
                CreateTableAsync(connection, "info_message", tblInfoMsg);
                CreateTableAsync(connection, "ethernet_connection_information", tblEthernetPortInfo);
                CreateTableAsync(connection, "ethernet_port_message", tblEthernetPortMsg);
            }
        }

        #region Query

        public async Task<int> QuerySerialPortInfoAsync(string portName, string baudRate, string parity, string dataBits, string stopBits, string handShake)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"SELECT port_id FROM serial_port_information 
                                                  WHERE port_name=$port_name AND baud_rate=$baud_rate AND parity=$parity AND
                                                        data_bits=$data_bits AND stop_bits=$stop_bits AND hand_shake=$hand_shake";
                    SqliteParameter paraPortName = new SqliteParameter("$port_name", portName);
                    SqliteParameter paraRate = new SqliteParameter("$baud_rate", baudRate);
                    SqliteParameter paraParity = new SqliteParameter("$parity", parity);
                    SqliteParameter paraDataBits = new SqliteParameter("$data_bits", dataBits);
                    SqliteParameter paraStopBits = new SqliteParameter("$stop_bits", stopBits);
                    SqliteParameter paraHandShake = new SqliteParameter("$hand_shake", handShake);

                    var command = await PrepareCommandAsync(connection, commandText, paraPortName, paraRate, paraParity, paraDataBits, paraStopBits, paraHandShake);
                    using (var reader = command.ExecuteReader())
                    {
                        return reader.Read() ? reader.GetInt32("port_id") : -1;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

        }

        public async Task<int> QueryEthernetPortInfoAsync(string workRole, string ipv4Address, string port, string maximum_clients, string receiveBufferSize)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"SELECT connection_id FROM ethernet_connection_information 
                                                        WHERE ipv4_address=$ipv4_address AND port=$port AND maximum_clients=$maximum_clients 
                                                                                         AND receive_buffer_size=$receive_buffer_size AND work_role=$work_role";
                    SqliteParameter paraAddress = new SqliteParameter("$ipv4_address", ipv4Address);
                    SqliteParameter paraPort = new SqliteParameter("$port", port);
                    SqliteParameter paraMaxiClients = new SqliteParameter("$maximum_clients", maximum_clients);
                    SqliteParameter paraBufferSize = new SqliteParameter("$receive_buffer_size", receiveBufferSize);
                    SqliteParameter paraRole = new SqliteParameter("$work_role", workRole);

                    var command = await PrepareCommandAsync(connection, commandText, paraAddress, paraPort, paraMaxiClients, paraBufferSize, paraRole);
                    using (var reader = command.ExecuteReader())
                    {
                        return reader.Read() ? reader.GetInt32("connection_id") : -1;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task<DataTable> QueryInfoMsgAsync(string source, int sourceID)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"SELECT info_id AS ID,info_level AS Level,info_content AS Content,info_time_stamp AS TimeStamp
                                                  FROM info_message 
                                                  WHERE info_source=$info_source AND info_source_id=$info_source_id ";
                    SqliteParameter paraSource = new SqliteParameter("$info_source", source);
                    SqliteParameter paraSourceID = new SqliteParameter("$info_source_id", sourceID);
                    var command = await PrepareCommandAsync(connection, commandText, paraSource, paraSourceID);

                    return await FillDataAsync(command);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task<DataTable> QuerySerialPortMsgAsync(int portID, string txOrRx)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"SELECT  message_id AS ID, content AS Content,time_stamp AS TimeStamp ,
                                                   port_name AS PortName, baud_rate AS BaudRate ,parity AS Parity,data_bits AS DataBits,stop_bits AS StopBits, hand_shake AS HandShake                                                 
                                                   FROM serial_port_message  
                                                   LEFT OUTER JOIN serial_port_information 
                                                   ON serial_port_message.port_id=serial_port_information.port_id 
                                                   WHERE serial_port_information.port_id=$port_id AND serial_port_message.send_receive=$send_receive ";
                    SqliteParameter paraPortID = new SqliteParameter("$port_id", portID);
                    SqliteParameter paraTxRx = new SqliteParameter("$send_receive", txOrRx);

                    var command = await PrepareCommandAsync(connection, commandText, paraPortID, paraTxRx);

                    return await FillDataAsync(command);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task<DataTable> QueryEthernetPortMsgAsync(int connectionID, string workRole, string txOrRx)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"SELECT  message_id AS ID, content AS Content,time_stamp AS TimeStamp ,remote_endpoint AS RemoteEndPoint,
                                                   work_role AS WorkRole, ipv4_address AS IPv4Address ,port AS Port,maximum_clients AS MaximumClients,
                                                   receive_buffer_size AS ReceiveBufferSize                                                   
                                                   FROM ethernet_port_message  
                                                   LEFT OUTER JOIN ethernet_connection_information 
                                                   ON ethernet_port_message.connection_id=ethernet_connection_information.connection_id 
                                                   WHERE ethernet_connection_information.connection_id=$connection_id AND 
                                                            ethernet_connection_information.work_role=$work_role AND 
                                                            ethernet_port_message.send_receive=$send_receive";
                    SqliteParameter paraConnectionID = new SqliteParameter("$connection_id", connectionID);
                    SqliteParameter paraRole = new SqliteParameter("$work_role", workRole);
                    SqliteParameter paraTxRx = new SqliteParameter("$send_receive", txOrRx);


                    var command = await PrepareCommandAsync(connection, commandText, paraConnectionID, paraRole, paraTxRx);

                    return await FillDataAsync(command);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        #endregion

        #region Insert

        public async Task<int> InsertSerialPortInfoAsync(string portName, string baudRate, string parity, string dataBits, string stopBits, string handShake)
        {
            int portID = await QuerySerialPortInfoAsync(portName, baudRate, parity, dataBits, stopBits, handShake);
            if (portID > 0)
            {
                return portID;
            }

            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string cmdText = @"INSERT OR IGNORE INTO serial_port_information(port_name,baud_rate,parity,data_bits,stop_bits,hand_shake) 
                                                             VALUES($port_name,$baud_rate,$parity,$data_bits,$stop_bits,$hand_shake)";
                    SqliteParameter paraPortName = new SqliteParameter("$port_name", portName);
                    SqliteParameter paraRate = new SqliteParameter("$baud_rate", baudRate);
                    SqliteParameter paraParity = new SqliteParameter("$parity", parity);
                    SqliteParameter paraDataBits = new SqliteParameter("$data_bits", dataBits);
                    SqliteParameter paraStopBits = new SqliteParameter("$stop_bits", stopBits);
                    SqliteParameter paraHandShake = new SqliteParameter("$hand_shake", handShake);

                    var command = await PrepareCommandAsync(connection, cmdText, paraPortName, paraRate, paraParity, paraDataBits, paraStopBits, paraHandShake);
                    int count = command.ExecuteNonQuery();
                    if (count > 0)
                    {
                        command = await PrepareCommandAsync(connection, @"SELECT last_insert_rowid()");
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        return -1;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task<int> InsertEthernetPortInfoAsync(string workRole, string ipv4Address, string port, string maximum_clients, string receiveBufferSize)
        {
            int connectionID = await QueryEthernetPortInfoAsync(workRole, ipv4Address, port, maximum_clients, receiveBufferSize);
            if (connectionID > 0)
            {
                return connectionID;
            }

            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"INSERT OR IGNORE INTO ethernet_connection_information(work_role, ipv4_address, port,maximum_clients, receive_buffer_size) 
                                                            VALUES($work_role, $ipv4_address, $port,$maximum_clients, $receive_buffer_size)";
                    SqliteParameter paraRole = new SqliteParameter("$work_role", workRole);
                    SqliteParameter paraAddress = new SqliteParameter("$ipv4_address", ipv4Address);
                    SqliteParameter paraPort = new SqliteParameter("$port", port);
                    SqliteParameter paraMaxiClients = new SqliteParameter("$maximum_clients", maximum_clients);
                    SqliteParameter paraBufferSize = new SqliteParameter("$receive_buffer_size", receiveBufferSize);

                    var command = await PrepareCommandAsync(connection, commandText, paraRole, paraAddress, paraPort, paraMaxiClients, paraBufferSize);
                    int count = command.ExecuteNonQuery();
                    if (count > 0)
                    {
                        command = await PrepareCommandAsync(connection, @"SELECT last_insert_rowid()");
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                    else
                    {
                        return -1;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task InsertSerialPortMsgAsync(int portID, string txOrRx, string content)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string cmdText = @"INSERT OR IGNORE INTO serial_port_message(port_id,send_receive,content,time_stamp) 
                                                            VALUES($port_id,$send_receive,$content,$time_stamp)";
                    SqliteParameter paraPortID = new SqliteParameter("$port_id", portID);
                    SqliteParameter paraTxRx = new SqliteParameter("$send_receive", txOrRx);
                    SqliteParameter paraContent = new SqliteParameter("$content", content);
                    SqliteParameter paraTimeStamp = new SqliteParameter("$time_stamp", DateTime.Now.ToLocalTime().ToString("yyyy-M-dd HH:mm:ss.FFF"));

                    var command = await PrepareCommandAsync(connection, cmdText, paraPortID, paraTxRx, paraContent, paraTimeStamp);
                    int count = command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task InsertEthernetPortMsgAsync(int connectionID, string txOrRx, string content, string remoteEndpoint)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"INSERT OR IGNORE INTO ethernet_port_message(connection_id, send_receive,content,time_stamp,remote_endpoint) 
                                                            VALUES($connection_id,$send_receive,$content,$time_stamp,$remote_endpoint)";
                    SqliteParameter paraConnectionID = new SqliteParameter("$connection_id", connectionID);
                    SqliteParameter paraTxRx = new SqliteParameter("$send_receive", txOrRx);
                    SqliteParameter paraContent = new SqliteParameter("$content", content);
                    SqliteParameter paraTimeStamp = new SqliteParameter("$time_stamp", DateTime.Now.ToLocalTime().ToString("yyyy-M-dd HH:mm:ss.FFF"));
                    SqliteParameter paraRemote = new SqliteParameter("$remote_endpoint", remoteEndpoint);


                    var command = await PrepareCommandAsync(connection, commandText, paraConnectionID, paraTxRx, paraContent, paraTimeStamp, paraRemote);
                    int count = command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task InsertInfoMsgAsync(string source, string content, int sourceID)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string level;
                    if (content.StartsWith("Error"))
                    {
                        level = "Error";
                    }
                    else if (content.StartsWith("Warning"))
                    {
                        level = "Warning";
                    }
                    else
                    {
                        level = "Notice";
                    }

                    string commandText = @"INSERT OR IGNORE INTO info_message(info_level, info_source,info_content,info_time_stamp,info_source_id) 
                                                            VALUES($info_level, $info_source,$info_content,$info_time_stamp,$info_source_id)";
                    SqliteParameter paraLevel = new SqliteParameter("$info_level", level);
                    SqliteParameter paraSource = new SqliteParameter("$info_source", source);
                    SqliteParameter paraContent = new SqliteParameter("$info_content", content);
                    SqliteParameter paraTimeStamp = new SqliteParameter("$info_time_stamp", DateTime.Now.ToLocalTime().ToString("yyyy-M-dd HH:mm:ss.FFF"));
                    SqliteParameter paraSourceID = new SqliteParameter("$info_source_id", sourceID);

                    var command = await PrepareCommandAsync(connection, commandText, paraLevel, paraSource, paraContent, paraTimeStamp, paraSourceID);
                    int count = command.ExecuteNonQuery();

                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        #endregion

        #region Delete

        public async void DeleteInfoMsgAsync(string source, int sourceID)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"DELETE FROM info_message WHERE info_source=$info_source AND info_source_id=$info_source_id ";
                    SqliteParameter paraSource = new SqliteParameter("$info_source", source);
                    SqliteParameter paraSourceID = new SqliteParameter("$info_source_id", sourceID);

                    var command = await PrepareCommandAsync(connection, commandText, paraSource, paraSourceID);
                    int count = command.ExecuteNonQuery();
                    if (count > 0)
                    {
                        await QueryInfoMsgAsync(source, sourceID);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async void DeleteSerialPortMsgAsync(int portID, string txOrRx)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"DELETE FROM serial_port_message 
                                                  WHERE port_id=$port_id AND send_receive=$send_receive ";
                    SqliteParameter paraPortID = new SqliteParameter("$port_id", portID);
                    SqliteParameter paraTxRx = new SqliteParameter("$send_receive", txOrRx);

                    var command = await PrepareCommandAsync(connection, commandText, paraPortID, paraTxRx);
                    int count = command.ExecuteNonQuery();
                    if (count > 0)
                    {
                        await QuerySerialPortMsgAsync(portID, txOrRx);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async void DeleteEthernetPortMsgAsync(int connectionID, string workRole, string txOrRx)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"DELETE FROM ethernet_port_message 
                                                  WHERE connection_id=$connection_id AND send_receive=$send_receive ";
                    SqliteParameter paraConnectionID = new SqliteParameter("$connection_id", connectionID);
                    SqliteParameter paraRole = new SqliteParameter("$work_role", workRole);
                    SqliteParameter paraTxRx = new SqliteParameter("$send_receive", txOrRx);

                    var command = await PrepareCommandAsync(connection, commandText, paraConnectionID, paraRole, paraTxRx);
                    int count = command.ExecuteNonQuery();
                    if (count > 0)
                    {
                        await QueryEthernetPortMsgAsync(connectionID, workRole, txOrRx);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        #endregion

        #region Update

        public async void UpdateEthernetPortInfoAsync(int connectionID, string ipv4Address, string port)
        {
            using (var connection = new SqliteConnection(connnectionCfg.ConnectionString))
            {
                try
                {
                    string commandText = @"UPDATE ethernet_connection_information SET ipv4_address=$ipv4_address, port=$port
                                                                                  WHERE connection_id=$connection_id ;";
                    SqliteParameter paraConnectionID = new SqliteParameter("$connection_id", connectionID);
                    SqliteParameter paraAddress = new SqliteParameter("$ipv4_address", ipv4Address);
                    SqliteParameter paraPort = new SqliteParameter("$port", port);

                    var command = await PrepareCommandAsync(connection, commandText, paraConnectionID, paraAddress, paraPort);
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        #endregion



    }


}
