using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WpfAppIndustryProtocolTestTool.Model;
using WpfAppIndustryProtocolTestTool.Model.Enum;

namespace WpfAppIndustryProtocolTestTool.BLL
{
    public class ToolHelper
    {
        public static string SetTime(bool displayDateTime, bool wordWrap)
        {
            if (displayDateTime)
            {
                return $"{SetWordWrap(!wordWrap)}>>> {DateTime.Now.ToLocalTime().ToString("yyyy-M-dd HH:mm:ss.fff")} <<< \n";
            }
            else
            {
                return string.Empty;
            }
        }


        public static string SetWordWrap(bool wordWrap)
        {
            return wordWrap ? Environment.NewLine : string.Empty;
        }


        public static uint CalcCountBytes(byte[] buffer, bool CountIncrement, uint currentCount)
        {

            if (CountIncrement)
            {
                currentCount += Convert.ToUInt16(buffer.Length);
            }
            else
            {
                currentCount = Convert.ToUInt16(buffer.Length);
            }
            return currentCount;
        }

        public static bool ReviewHexString(string hexString)
        {
            try
            {
                return Regex.IsMatch(hexString, @"([^A-Fa-f0-9]|\s+?)+");
            }
            catch (Exception)
            {

                throw;
            }

        }


        #region Hex <--> Byte[]
        public static byte[] strToHexByteArr(string hexString)
        {
            try
            {
                hexString = hexString.Replace(" ", "");
                if ((hexString.Length % 2) != 0)
                    hexString += " ";
                byte[] returnBytes = new byte[hexString.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                return returnBytes;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public static string byteArrToHexStr(byte[] bytes)
        {
            try
            {
                string returnStr = string.Empty;
                if (bytes != null)
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        returnStr += bytes[i].ToString("X2") + " ";
                    }
                }
                return returnStr;
            }
            catch (Exception)
            {

                throw;
            }

        }
        #endregion

        public static byte[] StringToByteArray(string message, DataFormatEnum dataFormat = DataFormatEnum.DEFAULT)
        {
            try
            {
                switch (dataFormat)
                {
                    case DataFormatEnum.ASCII:
                        return Encoding.ASCII.GetBytes(message);

                    case DataFormatEnum.HEX:
                        return strToHexByteArr(message);

                    case DataFormatEnum.UTF8:
                        return Encoding.UTF8.GetBytes(message);

                    default:
                        return Encoding.Default.GetBytes(message);

                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        public static string ByteArrayToString(byte[] buff, DataFormatEnum dataFormat = DataFormatEnum.DEFAULT)
        {
            try
            {
                switch (dataFormat)
                {
                    case DataFormatEnum.ASCII:
                        return Encoding.ASCII.GetString(buff);

                    case DataFormatEnum.HEX:
                        return byteArrToHexStr(buff);

                    case DataFormatEnum.UTF8:
                        return Encoding.UTF8.GetString(buff);

                    default:
                        return Encoding.Default.GetString(buff);

                }
            }
            catch (Exception)
            {

                throw;
            }


        }
    }
}
