using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperButton.ViewModels
{
    internal class WizardWindowViewModel : ViewModelBase
    {
        public WizardWindowViewModel() { }
        ~WizardWindowViewModel() { }

        private int _motorType = 0;
        public int MotorType {
            get { return _motorType; }
            set {
                _motorType = value;
                if(value == 1)
                    MotorFeedbacks = 0;
                else
                    MotorFeedbacks = 2;
                OnPropertyChanged("MotorType");
            }
        }
        private string _polePaire = "0";
        public string PolePaire
        {
            get { return _polePaire; }
            set { _polePaire = value; OnPropertyChanged("PolePaire"); }
        }
        private string _continuousCurrent = "0";
        public string ContinuousCurrent
        {
            get { return _continuousCurrent; }
            set { _continuousCurrent = value; OnPropertyChanged("ContinuousCurrent"); }
        }
        private string _motorSpeed = "0";
        public string MotorSpeed
        {
            get { return _motorSpeed; }
            set { _motorSpeed = value; OnPropertyChanged("MotorSpeed"); }
        }
        private int _motorFeedbacks = 2;
        public int MotorFeedbacks
        {
            get { return _motorFeedbacks; }
            set { _motorFeedbacks = value; OnPropertyChanged("MotorFeedbacks"); }
        }
        private string _cts_Motor = "0";
        public string cts_Motor
        {
            get { return _cts_Motor; }
            set { _cts_Motor = value; OnPropertyChanged("cts_Motor"); }
        }
    }
}
