using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace KitchenTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public int NewWindowCounter { get; set; } = 1;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetupExceptionHandling();
        }

        private void SetupExceptionHandling()
        {
            //AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            //    LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += App_DispatcherUnhandledException;

            //TaskScheduler.UnobservedTaskException += (s, e) =>
            //{
            //    LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
            //    e.SetObserved();
            //};
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Unexpected exception occurred: {e?.Exception.Message}", Constants.AppTitle);
            MessageBox.Show("Application will attempt to continue, you must need to restart it in some cases if errors continue.", Constants.AppTitle);
            e.Handled = true;
        }
    }
}
