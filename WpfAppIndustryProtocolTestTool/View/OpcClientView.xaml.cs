using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAppIndustryProtocolTestTool.View
{
    /// <summary>
    /// Interaction logic for OPC_ClassicView.xaml
    /// </summary>
    public partial class OpcClientView : UserControl
    {
        public OpcClientView()
        {
            InitializeComponent();
        }

        public IEnumerable<T> GetChildren<T>(DependencyObject p_element, Func<T, bool>? p_func = null) where T : UIElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(p_element); i++)
            {
                UIElement? child = VisualTreeHelper.GetChild(p_element, i) as FrameworkElement;
                if (child == null)
                {
                    continue;
                }

                if (child is T)
                {
                    var t = (T)child;
                    if (p_func != null && !p_func(t))
                    {
                        continue;
                    }

                    yield return t;
                }
                else
                {
                    foreach (var c in GetChildren(child, p_func))
                    {
                        yield return c;
                    }
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var item in GetChildren<CheckBox>(dataGrid))
            {
                item.IsChecked = true;
            }
            
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in GetChildren<CheckBox>(dataGrid))
            {
                item.IsChecked = false;
            }
        }
    }
}
