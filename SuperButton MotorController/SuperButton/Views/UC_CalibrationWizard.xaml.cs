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

namespace SuperButton.Views
{
    /// <summary>
    /// Interaction logic for UC_CalibrationWizard.xaml
    /// </summary>
    using System.Windows;
    using System.Windows.Controls;
    public partial class UC_CalibrationWizard : UserControl
    {
        private bool blinking;

        public UC_CalibrationWizard()
        {
            this.InitializeComponent();
            //IsBlinking = true;
        }

        public bool IsBlinking
        {
            get
            {
                return blinking;
            }

            set
            {
                if(value)
                {
                    VisualStateManager.GoToState(this, "Blinking", true);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Stopped", true);
                }

                this.blinking = value;
            }
        }
    }
}
