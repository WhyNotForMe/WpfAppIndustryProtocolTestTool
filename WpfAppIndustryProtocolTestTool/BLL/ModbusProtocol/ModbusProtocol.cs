using System;

namespace WpfAppIndustryProtocolTestTool.BLL.ModbusProtocol
{
    internal class ModbusProtocol
    {
        public enum ProtocolType
        {
            ModbusTCP,
            ModbusUDP,
            ModbusRTU
        }

        public DateTime timeStamp;

        public bool request;

        public bool response;

        public ushort transactionIdentifier;

        public ushort protocolIdentifier;

        public ushort length;

        public byte unitIdentifier;

        public byte functionCode;

        public ushort startingAdress;

        public ushort startingAddressRead;

        public ushort startingAddressWrite;

        public ushort quantity;

        public ushort quantityRead;

        public ushort quantityWrite;

        public byte byteCount;

        public byte exceptionCode;

        public byte errorCode;

        public ushort[] receiveCoilValues;

        public ushort[] receiveRegisterValues;

        public short[] sendRegisterValues;

        public bool[] sendCoilValues;

        public ushort crc;
    }
}
