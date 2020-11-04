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
using System.Windows.Shapes;
using MotorController.ViewModels;
using MotorController.Models.DriverBlock;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows.Interop;

namespace MotorController.Windows
{
    /// <summary>
    /// Interaction logic for ParametarsWindow.xaml
    /// </summary>
    public partial class ParametarsWindow : Window
    {
        public ParametarsWindow()
        {
            InitializeComponent();
        }
        private new void MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
        private void FolderTB_ScrolltoEnd(object sender, EventArgs e)
        {
            ((TextBox)sender).ScrollToHorizontalOffset(((TextBox)sender).ExtentWidth);
        }
    }
}
