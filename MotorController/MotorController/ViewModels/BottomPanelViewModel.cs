using MotorController.Common;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MotorController.Models;
using Abt.Controls.SciChart;
using MotorController.Models.DriverBlock;
using System;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;

namespace MotorController.ViewModels
{
    public class BottomPanelViewModel : ViewModelBase
    {
        public BottomPanelViewModel()
        {

        }
        private static BottomPanelViewModel _instance;
        private static readonly object Synlock = new object();
        public static BottomPanelViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new BottomPanelViewModel();
                    return _instance;
                }
            }
        }
        private ObservableCollection<object> _motionCommandList;
        private ObservableCollection<object> _motionCommandList2;

        private ObservableCollection<object> _motionStatusList;
        private ObservableCollection<object> _digitalInputList;
        private ObservableCollection<object> _SGList;
        private ObservableCollection<object> _positionCountersList;

        public ObservableCollection<object> MotionCommandList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["MotionCommand List"]; //Motion Limit
            }
            set
            {
                _motionCommandList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> MotionCommandList2
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["MotionCommand List2"]; //Motion Limit
            }
            set
            {
                _motionCommandList2 = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<object> ToggleOperationList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["BP_ToggleSwitch"];
            }
        }
        public ObservableCollection<object> DigitalInputList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Digital Input List"]; //Motion Limit
            }
            set
            {
                _digitalInputList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> PositionCountersList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Position counters List"]; //Motion Limit
            }
            set
            {
                _positionCountersList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<object> MotionStatusList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["MotionStatus List"];
            }
            set
            {
                _motionStatusList = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<object> ControlList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Control"];
            }
        }

        public ObservableCollection<object> MotorlList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Motor"];
            }
        }
        public ObservableCollection<object> ProfilerModeList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["Profiler Mode"];
            }
        }
        public ObservableCollection<object> SGTypeList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["S.G.Type"];
            }
        }
        public ObservableCollection<object> SGList
        {
            get
            {
                return Commands.GetInstance.GenericCommandsGroup["S.G.List"];
            }
            set
            {
                _SGList = value;
                OnPropertyChanged();
            }
        }
    }
}
