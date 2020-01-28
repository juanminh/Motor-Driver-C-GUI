using Abt.Controls.SciChart;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperButton.ViewModels
{
    enum MODE
    {
        Normal = 0,
        Advanced = 1
    };
    internal class WizardWindowViewModel : ViewModelBase
    {
        public Dictionary<Tuple<string>, CalibrationWizardViewModel> CalibrationWizardList = new Dictionary<Tuple<string>, CalibrationWizardViewModel>();
        public Dictionary<string, ObservableCollection<object>> CalibrationWizardListbySubGroup = new Dictionary<string, ObservableCollection<object>>();

        public WizardWindowViewModel()
        {
            CalibrationWizardListbySubGroup.Add("CalibrationList", new ObservableCollection<object>());
            BuildCalibrationWizardList();
        }
        ~WizardWindowViewModel() { }
        #region Motor_Parameter
        private int _motorType = 0;
        public int MotorType
        {
            get { return _motorType; }
            set
            {
                _motorType = value;
                if(value == 1)
                    MotorFeedbacks = 0;
                else
                    MotorFeedbacks = 2;
                BuildCalibrationWizardList();
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
        #endregion Motor_Parameter
        #region Calibration
        private ObservableCollection<object> _calibList;
        public ObservableCollection<object> CalibList
        {
            get
            {
                return CalibrationWizardListbySubGroup["CalibrationList"];
            }
            set
            {
                _calibList = value;
                OnPropertyChanged();
            }

        }

        private void BuildCalibrationWizardList()
        {
            CalibrationWizardList.Clear();
            CalibrationWizardListbySubGroup["CalibrationList"].Clear();

            var names = new[]
            {
                "PI Current Loop", "Commutation Angle", "Feedback Direction", "PI Speed Loop", "PI Position Loop"
            };
            CalibrationWizardViewModel calibElement;
            for(int i = 0; i < names.Length; i++)
            {
                calibElement = new CalibrationWizardViewModel
                {
                    AdvanceMode_Calibration = CalibrationAdvancedMode,
                    CalibrationPerform = true,
                    CalibrationName = names[i],
                    CalibStatus = 0
                };
                CalibrationWizardList.Add(new Tuple<string>(names[i]), calibElement);
                CalibrationWizardListbySubGroup["CalibrationList"].Add(calibElement);
                if(MotorType == 0 && i == 0)
                    i++;
            }
        }
        public ActionCommand Start { get { return new ActionCommand(StartCalib); } }
        private void StartCalib()
        {

        }
        public ActionCommand Abort { get { return new ActionCommand(AbortCalib); } }
        private void AbortCalib()
        {

        }
        #endregion Calibration
        #region CalibrationAdvancedMode
        private bool _calibrationAdvancedMode = false;
        public bool CalibrationAdvancedMode
        {
            get { return _calibrationAdvancedMode; }
            set
            {
                _calibrationAdvancedMode = value;
                BuildCalibrationWizardList();
                OnPropertyChanged("CalibrationAdvancedMode");
            }
        }
        #endregion CalibrationAdvancedMode

    }
}
