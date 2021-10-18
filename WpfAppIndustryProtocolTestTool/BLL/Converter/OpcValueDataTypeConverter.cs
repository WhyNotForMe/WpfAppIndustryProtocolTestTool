using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfAppIndustryProtocolTestTool.BLL.Converter
{
    internal class OpcValueDataTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string dataType = value as string;
            if (!string.IsNullOrEmpty(dataType))
            {
                switch (dataType)
                {
                    case "0":
                        return "Default";
                    case "2":
                        return "Short";
                    case "3":
                        return "Int";
                    case "4":
                        return "Float";
                    case "5":
                        return "Double";
                    case "6":
                        return "Currency";
                    case "7":
                        return "Date";
                    case "8":
                        return "Text(Unicode)";
                    case "10":
                        return "ErrorCode";
                    case "11":
                        return "Bool";
                    case "16":
                        return "Byte";
                    case "17":
                        return "UByte";
                    case "18":
                        return "UShort";
                    case "19":
                        return "UInt";
                    case "8194":
                        return "Short[ ]";
                    case "8195":
                        return "Int[ ]";
                    case "8196":
                        return "Float[ ]";
                    case "8197":
                        return "Double[ ]";
                    case "8198":
                        return "Currency[ ]";
                    case "8199":
                        return "Date[ ]";
                    case "8200":
                        return "Text[ ]";
                    case "8202":
                        return "ErrorCode[ ]";
                    case "8203":
                        return "Bool[ ]";
                    case "8208":
                        return "Byte[ ]";
                    case "8209":
                        return "UByte[ ]";
                    case "8210":
                        return "UShort[ ]";
                    case "8211":
                        return "UInt[ ]";
                    default:
                        return dataType;
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
