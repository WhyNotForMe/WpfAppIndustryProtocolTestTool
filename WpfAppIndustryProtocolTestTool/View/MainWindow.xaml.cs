using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using WpfAppIndustryProtocolTestTool.BLL;
using WpfAppIndustryProtocolTestTool.ViewModel;

namespace WpfAppIndustryProtocolTestTool.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<string>(this, "Close", (close) =>
            {
                GlobalViewManager.RemoveView("SerialPort");
                GlobalViewManager.RemoveView("TcpUdp");
                GlobalViewManager.RemoveView("Modbus");
                GlobalViewManager.RemoveView("OpcClient");

                ViewModelLocator.Cleanup<SerialPortViewModel>();
                ViewModelLocator.Cleanup<TcpUdpViewModel>();
                ViewModelLocator.Cleanup<ModbusViewModel>();
                ViewModelLocator.Cleanup<OpcClientViewModel>();

                if (close == "CloseWindow")
                {
                    GlobalViewManager.RemoveView("FirstPage");

                    ViewModelLocator.Cleanup<FirstPageViewModel>();
                    ViewModelLocator.Cleanup<MainWindowViewModel>();

                    this.Close();
                }

            });

        }



        private void WindowMaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
        }

        private void WindowMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void WindowDragMove_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }


    }
}
