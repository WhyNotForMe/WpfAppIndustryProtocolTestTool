
using System;
using System.Threading.Tasks;
using System.Windows;

namespace WpfAppIndustryProtocolTestTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            base.OnStartup(e);

        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            MessageBox.Show($"{e.Exception.Message}\n\n{e.Exception.StackTrace}", "Task Scheduler Unobserved Task Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.SetObserved();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {   //2nd triggered due to unhandled of Current_DispatcherUnhandledException.
            //Exception ex = e.ExceptionObject as Exception;
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Current Domain Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {   //1st triggered.
            MessageBox.Show($"{e.Exception.Message}\n\n{e.Exception.StackTrace}", "Dispatcher Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }


    }
}
