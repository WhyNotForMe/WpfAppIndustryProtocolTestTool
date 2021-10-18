/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:WpfAppIndustryProtocolTestTool"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;

namespace WpfAppIndustryProtocolTestTool.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<FirstPageViewModel>();
            SimpleIoc.Default.Register<SerialPortViewModel>();
            SimpleIoc.Default.Register<TcpUdpViewModel>();
            SimpleIoc.Default.Register<ModbusViewModel>();
            SimpleIoc.Default.Register<OpcClientViewModel>();

        }

        public MainWindowViewModel MainWindow
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainWindowViewModel>();
            }
        }

        public FirstPageViewModel FirstPage
        {
            get
            {
                return ServiceLocator.Current.GetInstance<FirstPageViewModel>();
            }
        }

        public SerialPortViewModel SerialPort
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SerialPortViewModel>();
            }
        }

        public TcpUdpViewModel TcpUdp
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TcpUdpViewModel>();
            }
        }

        public ModbusViewModel Modbus
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ModbusViewModel>();
            }
        }

        public OpcClientViewModel OPC
        {
            get
            {
                return ServiceLocator.Current.GetInstance<OpcClientViewModel>();
            }
        }

        public static void Cleanup<T>() where T : ViewModelBase
        {
            // TODO Clear the ViewModels
            SimpleIoc.Default.Unregister<T>();
            SimpleIoc.Default.Register<T>();

            //Usage for another place
            //ViewModelLocator.Cleanup<OpcClientViewModel>();

        }
    }
}