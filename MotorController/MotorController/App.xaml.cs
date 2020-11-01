//#define LOAD_FROM_DB
using Abt.Controls.SciChart.Visuals;
using MotorController.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Security.AccessControl;
using System.IO;
using System.Security.Principal;
using MotorController.Helpers;
using System.Threading.Tasks;
using MotorController.Views;

namespace MotorController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Ensure SetLicenseKey is called once, before any SciChartSurface instance is created 
        // Check this code into your version-control and it will enable SciChart 
        // for end-users of your application. 
        // 
        // You can test the Runtime Key is installed correctly by Running your application 
        // OUTSIDE Of Visual Studio (no debugger attached). Trial watermarks should be removed. 


        public App()
        {
            SciChartSurface.SetRuntimeLicenseKey(@"<LicenseContract>
  <Customer>Redler technologies</Customer>
  <OrderId>ABT141014-5754-30127</OrderId>
  <LicenseCount>1</LicenseCount>
  <IsTrialLicense>false</IsTrialLicense>
  <SupportExpires>01/12/2015 00:00:00</SupportExpires>
  <ProductCode>SC-WPF-BSC</ProductCode>
  <KeyCode>lwAAAQEAAADYej6WZT7WAYsAQ3VzdG9tZXI9UmVkbGVyIHRlY2hub2xvZ2llcztPcmRlcklkPUFCVDE0MTAxNC01NzU0LTMwMTI3O1N1YnNjcmlwdGlvblZhbGlkVG89MTItSmFuLTIwMTU7UHJvZHVjdENvZGU9U0MtV1BGLUJTQztOdW1iZXJEZXZlbG9wZXJzT3ZlcnJpZGU9MYHBQsFtvhmNUsAF1tPpbfJI0MXhteDAzO1I1uzwGcNIr/3e8pkIaMWJiXsaX6Q0Ew==</KeyCode>
</LicenseContract>");

#if LOAD_FROM_DB 
            Operations Op = Operations.GetInstance;
            Operations.GetInstance.readDataBase();
#endif
            LeftPanelViewModel.GetInstance.LogText = "";
            EventRiser.Instance.LoggerEvent += LeftPanelViewModel.GetInstance.Instance_LoggerEvent;

            //EventRiser.Instance.RiseEevent(string.Format($"App Started"));

            Startup += new StartupEventHandler(App_Startup); // Can be called from XAML 

            DispatcherUnhandledException += App_DispatcherUnhandledException; //Example 2 

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; //Example 4 

            System.Windows.Forms.Application.ThreadException += WinFormApplication_ThreadException; //Example 5 
        }
        void App_Startup(object sender, StartupEventArgs e)
        {
            //Here if called from XAML, otherwise, this code can be in App() 
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException; // Example 1 
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; // Example 3 
        }
        // Example 1 
        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            StackTrace stackTrace = new StackTrace();
            ErrorLog(stackTrace, e.Exception, "void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)");
            if(e.Exception.Message != "Application identity is not set.")
                EventRiser.Instance.RiseEevent(string.Format(e.Exception.Message));
        }

        // Example 2 
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            StackTrace stackTrace = new StackTrace();
            ErrorLog(stackTrace, e.Exception, "void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)");
            EventRiser.Instance.RiseEevent(string.Format(e.Exception.Message));

            e.Handled = true;
        }

        // Example 3 
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            StackTrace stackTrace = new StackTrace();
            ErrorLog(stackTrace, exception, "void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)");

            EventRiser.Instance.RiseEevent(string.Format(exception.Message));

            //if(e.IsTerminating)
            {
                //Now is a good time to write that critical error file! 
                //MessageBox.Show("Goodbye world!");
            }
        }

        // Example 4 
        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            StackTrace stackTrace = new StackTrace();
            ErrorLog(stackTrace, e.Exception, "void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)");
            EventRiser.Instance.RiseEevent(string.Format(e.Exception.Message));

            e.SetObserved();
        }

        // Example 5 
        void WinFormApplication_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            StackTrace stackTrace = new StackTrace();
            ErrorLog(stackTrace, e.Exception, "void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)");
            EventRiser.Instance.RiseEevent(string.Format(e.Exception.Message));

        }


        protected override void OnStartup(StartupEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;

            // raise selection change event even when there's no change in index
            //EventManager.RegisterClassHandler(typeof(ComboBoxItem), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(ComboBoxSelfSelection), true);

            base.OnStartup(e);
        }

        private static void ComboBoxSelfSelection(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ComboBoxItem;

            if(item == null)
                return;

            // find the combobox where the item resides
            var comboBox = ItemsControl.ItemsControlFromItemContainer(item) as System.Windows.Controls.ComboBox;

            if(comboBox == null)
                return;

            // fire SelectionChangedEvent if two value are the same
            if((string)comboBox.Name != "ComboboxCOM")
            {
                if((string)comboBox.SelectedValue.ToString() == (string)item.Content)
                {
                    comboBox.IsDropDownOpen = false;
                    comboBox.RaiseEvent(new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), new List<object>()));
                }
            }
        }

        private void ErrorLog(StackTrace _stackTrace, Exception _exception, String _functionName)
        {
            string Date = OscilloscopeViewModel.Day(DateTime.Now.Day) + ' ' + OscilloscopeViewModel.MonthTrans(DateTime.Now.Month) + ' ' + DateTime.Now.Year.ToString();
            string path = "\\ErrorLog\\";
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + path;

            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = "\\ErrorLog\\";
            path += Date + ' ' + DateTime.Now.ToString("HH:mm:ss");
            path = (path.Replace('-', ' ')).Replace(':', '_');
            path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + path;
            path += ".txt";


            try
            {
                if(!Directory.Exists(path))
                {
                    using(StreamWriter writer = new StreamWriter(path))
                    {
                        writer.WriteLine("***  Function Name ***");
                        writer.WriteLine(_functionName);
                        writer.WriteLine(_exception.Message);
                        writer.WriteLine("***  Function Name ***");

                        for(int i = _stackTrace.FrameCount - 1; i >= 0; i--)
                        {
                            writer.WriteLine(_stackTrace.GetFrame(i).GetMethod().Name);
                        }
                    }
                }
            }
            catch { }
        }
    }
}