using Abt.Controls.SciChart.Visuals;
using SuperButton.ViewModels;
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

namespace SuperButton
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
            //SciChartSurface.SetRuntimeLicenseKey(@"<LicenseContract>
            //<Customer>Redler technologies</Customer>
            //<OrderId>ABT141014-5754-30127</OrderId>
            //<LicenseCount>1</LicenseCount>
            //<IsTrialLicense>false</IsTrialLicense>
            //<SupportExpires>01/12/2015 00:00:00</SupportExpires>
            //<ProductCode>SC-WPF-BSC</ProductCode>
            //<KeyCode>lwAAAAEAAAAYTULLhErUAXAAQ3VzdG9tZXI9UmVkbGVyIHRlY2hub2xvZ2llcztPcmRlcklkPUFCVDE0MTAxNC01NzU0LTMwMTI3O1N1YnNjcmlwdGlvblZhbGlkVG89MTItSmFuLTIwMTU7UHJvZHVjdENvZGU9U0MtV1BGLUJTQyu69TgpwVx+uxEH2B+6rKOQ/5YDD2Oh+vDxAZ3OzX+X05jc9xhuF7mPcAXFaqyfWA==</KeyCode>
            //</LicenseContract>");
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;

            // raise selection change event even when there's no change in index
            EventManager.RegisterClassHandler(typeof(ComboBoxItem), UIElement.PreviewMouseLeftButtonDownEvent,
                                              new MouseButtonEventHandler(ComboBoxSelfSelection), true);
            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));

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
            if((string)comboBox.SelectedValue.ToString() == (string)item.Content)
            {
                comboBox.IsDropDownOpen = false;
                comboBox.RaiseEvent(new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), new List<object>()));
            }
        }
        static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            /*
            Trace.WriteLine("Clicked!!");
            if(LeftPanelViewModel.win != null)
            {
                if((LeftPanelViewModel.win.WindowState == WindowState.Normal || LeftPanelViewModel.win.WindowState == WindowState.Maximized) &&
                    (App.Current.MainWindow.WindowState == WindowState.Normal || App.Current.MainWindow.WindowState == WindowState.Maximized)
                    )
                {
                    var temp = sender as Window;
                    if(temp.Name == "MainAppWindow")
                    {
                        //LeftPanelViewModel.win.WindowState = WindowState.Minimized;
                    }
                }
            }
            */
        }
    }
}