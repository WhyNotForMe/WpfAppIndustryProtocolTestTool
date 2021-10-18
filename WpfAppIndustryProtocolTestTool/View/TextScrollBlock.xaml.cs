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
    /// Interaction logic for TextScrollBlock.xaml
    /// </summary>
    public partial class TextScrollBlock : UserControl
    {
        public TextScrollBlock()
        {
            InitializeComponent();
        }


        private void textBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            scrollViewer.ScrollToVerticalOffset(textBlock.ActualHeight);
        }




        public string TextContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register("TextContent", typeof(string), typeof(TextScrollBlock), new PropertyMetadata(default(string)));





    }

}
