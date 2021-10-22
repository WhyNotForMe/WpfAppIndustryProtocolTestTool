using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfAppIndustryProtocolTestTool.BLL.Converter
{
    internal class TextToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? textStr = value as string;
            if (!string.IsNullOrEmpty(textStr))
            {
                if (textStr.StartsWith("Warning"))
                {
                    return new SolidColorBrush(Colors.Yellow);
                }
                else if (textStr.StartsWith("Error"))
                {
                    return new SolidColorBrush(Colors.OrangeRed);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                return null;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
