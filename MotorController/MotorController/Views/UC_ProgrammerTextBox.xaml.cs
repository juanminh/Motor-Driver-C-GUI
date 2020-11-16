using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace MotorController.Views
{
    /// <summary>
    /// Interaction logic for ProgrammerTextBox.xaml
    /// </summary>
    public partial class UC_ProgrammerTextBox : UserControl
    {
        public UC_ProgrammerTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextStrProperty = DependencyProperty.Register(
            "TextStr", typeof(string), typeof(UC_ProgrammerTextBox));
        public string TextStr
        {
            get { return (string)GetValue(TextStrProperty); }
            set { SetValue(TextStrProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(UC_ProgrammerTextBox));
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
    }
}
