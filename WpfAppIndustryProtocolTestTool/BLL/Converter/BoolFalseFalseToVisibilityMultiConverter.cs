using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfAppIndustryProtocolTestTool.BLL.Converter
{
    public class BoolFalseFalseToVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] != null && values[1] != null)
            {
                if ((bool)values[0] == false && (bool)values[1] == false)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
