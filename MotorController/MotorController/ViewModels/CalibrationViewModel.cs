using Abt.Controls.SciChart;
using MotorController.Models.DriverBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;
using MotorController.Common;

namespace MotorController.ViewModels
{
    internal class CalibrationViewModel : ViewModelBase
    {
        #region FIELDS
        private static readonly object Synlock = new object();
        private static CalibrationViewModel _instance;
        #endregion FIELDS
        
        public static CalibrationViewModel GetInstance
        {
            get
            {
                lock (Synlock)
                {
                    if (_instance != null) return _instance;
                    _instance = new CalibrationViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        private CalibrationViewModel()
        {

        }

        public ObservableCollection<object> CalibrationList_ToggleSwitch
        {

            get
            {
                return Commands.GetInstance.GenericCommandsGroup["CalibrationList_ToggleSwitch"];
            }
        }
        public ObservableCollection<object> CalibrationResultList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Calibration Result List"];
            }
        }
    }
}






