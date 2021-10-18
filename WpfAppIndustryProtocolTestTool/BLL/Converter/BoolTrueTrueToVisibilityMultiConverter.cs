using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfAppIndustryProtocolTestTool.BLL.Converter
{
    public class BoolTrueTrueToVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] != null && values[1] != null)
            {
                if ((bool)values[0] == true && (bool)values[1] == true)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;


        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
