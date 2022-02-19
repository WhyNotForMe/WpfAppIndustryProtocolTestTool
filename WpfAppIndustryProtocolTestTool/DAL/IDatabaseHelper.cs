using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIndustryProtocolTestTool.DAL
{
    internal interface IDatabaseHelper
    {
        Task<int> QuerySerialPortInfoAsync(string portName, string baudRate, string parity, string dataBits, string stopBits, string handShake);
        Task<int> QueryEthernetPortInfoAsync(string workRole, string ipv4Address, string port, string maximum_clients, string receiveBufferSize);
        Task<DataTable> QueryInfoMsgAsync(string source, int sourceID);
        Task<DataTable> QuerySerialPortMsgAsync(int portID, string txOrRx);
        Task<DataTable> QueryEthernetPortMsgAsync(int connectionID, string workRole, string txOrRx);

        Task<int> InsertSerialPortInfoAsync(string portName, string baudRate, string parity, string dataBits, string stopBits, string handShake);
        Task<int> InsertEthernetPortInfoAsync(string workRole, string ipv4Address, string port, string maximum_clients, string receiveBufferSize);
        Task InsertSerialPortMsgAsync(int portID, string txOrRx, string content);
        Task InsertEthernetPortMsgAsync(int connectionID, string txOrRx, string content, string remoteEndpoint);
        Task InsertInfoMsgAsync(string source, string content, int sourceID);

        void DeleteInfoMsgAsync(string source, int sourceID);
        void DeleteSerialPortMsgAsync(int portID, string txOrRx);
        void DeleteEthernetPortMsgAsync(int connectionID, string workRole, string txOrRx);

        void UpdateEthernetPortInfoAsync(int connectionID, string ipv4Address, string port);
    }
}
