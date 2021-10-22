using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfAppIndustryProtocolTestTool.BLL.Converter
{
    internal class OpcValueQualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? quality = value as string;
            if (!string.IsNullOrEmpty(quality))
            {
                switch (quality)
                {
                    case "0":
                        return "BAD";
                    case "C0":
                        return "GOOD";
                    case "D8":
                        return "LOCAL_OVERRIDE";
                    case "04":
                        return "CONFIG_ERROR";
                    case "08":
                        return "NOT_CONNECTED";
                    case "0C":
                        return "DEVICE_FAILURE";
                    case "14":
                        return "LAST_KNOWN";
                    case "18":
                        return "COMM_FAILURE";
                    case "1C":
                        return "OUT_OF_SERVICE";
                    case "10":
                        return "SENSOR_FAILURE";
                    default:

                        return quality;
                }
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
