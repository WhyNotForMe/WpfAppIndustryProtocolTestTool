using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfAppIndustryProtocolTestTool.Model.Enum;

namespace WpfAppIndustryProtocolTestTool.BLL
{
    public class CRCHelper
    {
        public static byte GetCRC8(byte[] buffer)
        {
            byte crc = 0;
            for (int j = 0; j < buffer.Length; j++)
            {
                crc ^= buffer[j];
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x01) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0x8c;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }


        public static byte[] GetCRC16(byte[] data, bool isReverse = false)
        {
            int len = data.Length;
            if (len > 0)
            {
                ushort crc = 0xFFFF;

                for (int i = 0; i < len; i++)
                {
                    crc = (ushort)(crc ^ (data[i]));
                    for (int j = 0; j < 8; j++)
                    {
                        crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                    }
                }
                byte hi = (byte)((crc & 0xFF00) >> 8);
                byte lo = (byte)(crc & 0x00FF);

                return isReverse ? new byte[] { hi, lo } : new byte[] { lo, hi };
            }
            return new byte[] { 0, 0 };
        }


        #region CRC32

        static protected uint[]? CRC32Table;
        static public void GetCRC32Table()
        {
            uint Crc;
            CRC32Table = new uint[256];
            int i, j;
            for (i = 0; i < 256; i++)
            {
                Crc = (uint)i;
                for (j = 8; j > 0; j--)
                {
                    if ((Crc & 1) == 1)
                        Crc = (Crc >> 1) ^ 0xEDB88320;
                    else
                        Crc >>= 1;
                }
                CRC32Table[i] = Crc;
            }
        }
        static public byte[] GetCRC32(byte[] InputArray)
        {
            try
            {
                GetCRC32Table();
                uint value = 0xffffffff;
                int len = InputArray.Length;
                for (int i = 0; i < len; i++)
                {
                    value = (value >> 8) ^ CRC32Table[(value & 0xFF) ^ InputArray[i]];
                }
                return BitConverter.GetBytes(value ^ 0xffffffff);
            }
            catch (Exception)
            {

                throw;
            }

        }

        #endregion




        public static byte[] AppendCRC(byte[] rawBuffer, CRCEnum cRC)
        {
            try
            {

                byte[] newBuffer;

                switch (cRC)
                {

                    case CRCEnum.CRC8:
                        newBuffer = new byte[rawBuffer.Length + 1];
                        Array.Copy(rawBuffer, 0, newBuffer, 0, rawBuffer.Length);
                        newBuffer[rawBuffer.Length] = GetCRC8(rawBuffer);

                        return newBuffer;
                    case CRCEnum.CRC16:
                        newBuffer = new byte[rawBuffer.Length + 2];
                        Array.Copy(rawBuffer, 0, newBuffer, 0, rawBuffer.Length);
                        byte[] cRC16 = GetCRC16(rawBuffer);
                        Array.Copy(cRC16, 0, newBuffer, rawBuffer.Length, 2);

                        return newBuffer;
                    case CRCEnum.CRC32:
                        newBuffer = new byte[rawBuffer.Length + 4];
                        Array.Copy(rawBuffer, 0, newBuffer, 0, rawBuffer.Length);
                        byte[] cRC32 = GetCRC32(rawBuffer);
                        Array.Copy(cRC32, 0, newBuffer, rawBuffer.Length, 4);

                        return newBuffer;
                    default:

                        return rawBuffer;
                }



            }
            catch (Exception)
            {

                throw;
            }




        }


        public static byte[] ValidateCRC(byte[] rawBuffer, CRCEnum cRC)
        {
            try
            {
                byte[] actualData;
                byte[] calcValue;

                switch (cRC)
                {
                    case CRCEnum.CRC8:
                        actualData = new byte[rawBuffer.Length - 1];
                        Array.Copy(rawBuffer, 0, actualData, 0, rawBuffer.Length - 1);
                        if (GetCRC8(actualData) == rawBuffer[rawBuffer.Length - 1])
                        {
                            return actualData;
                        }
                        else
                        {
                            return null;
                        }
                    case CRCEnum.CRC16:
                        actualData = new byte[rawBuffer.Length - 2];
                        Array.Copy(rawBuffer, 0, actualData, 0, rawBuffer.Length - 2);
                        calcValue = GetCRC16(actualData);
                        if (BitConverter.ToInt16(calcValue, 0) == BitConverter.ToInt16(rawBuffer, rawBuffer.Length - 2))
                        {
                            return actualData;
                        }
                        else
                        {
                            return null;
                        }
                    case CRCEnum.CRC32:
                        actualData = new byte[rawBuffer.Length -4];
                        Array.Copy(rawBuffer, 0, actualData, 0, rawBuffer.Length - 4);
                        calcValue = GetCRC32(actualData);
                        if (BitConverter.ToInt32(calcValue, 0) == BitConverter.ToInt32(rawBuffer, rawBuffer.Length - 4))
                        {
                            return actualData;
                        }
                        else
                        {
                            return null;
                        }
                    default:

                        return rawBuffer;
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
    }



}
