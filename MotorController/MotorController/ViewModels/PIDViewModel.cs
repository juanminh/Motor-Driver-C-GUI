using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MotorController.CommandsDB;
using System.Diagnostics;
using MotorController.Models.DriverBlock;

namespace MotorController.ViewModels
{
    public class PIDViewModel : ViewModelBase
    {
        private static readonly object Synlock = new object();
        private static PIDViewModel _instance;

        public static PIDViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new PIDViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }

        }
        private PIDViewModel()
        {

        }


        #region PID
        private ObservableCollection<object> _pidCurrentList;
        public ObservableCollection<object> PidCurrentList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["PIDCurrent"];
            }
            set
            {
                _pidCurrentList = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<object> _pidSpeedList;
        public ObservableCollection<object> PidSpeedList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["PIDSpeed"];
            }
            set
            {
                _pidSpeedList = value;
                OnPropertyChanged();
            }

        }
        private ObservableCollection<object> _pidPositionList;
        public ObservableCollection<object> PidPositionList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["PIDPosition"];
            }
            set
            {
                _pidPositionList = value;
                OnPropertyChanged();
            }

        }
        private ObservableCollection<object> _PID_speed_loop;
        public ObservableCollection<object> PID_speed_loop
        {

            get
            {
                return Commands.GetInstance.ToggleSwitchList["PID_speed_loop"];
            }
            set
            {
                _PID_speed_loop = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<object> _PID_current_loop;
        public ObservableCollection<object> PID_current_loop
        {

            get
            {
                return Commands.GetInstance.ToggleSwitchList["PID_current_loop"];
            }
            set
            {
                _PID_current_loop = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<object> _PID_position_loop;
        public ObservableCollection<object> PID_position_loop
        {

            get
            {
                return Commands.GetInstance.ToggleSwitchList["PID_position_loop"];
            }
            set
            {
                _PID_position_loop = value;
                OnPropertyChanged();
            }
        }
        //private bool _PID_current_loop = false;

        //public bool PID_current_loop
        //{
        //    get
        //    {
        //        return _PID_current_loop;
        //    }
        //    set
        //    {
        //        if(!LeftPanelViewModel._app_running)
        //            return;
        //        // get call stack
        //        StackTrace stackTrace = new StackTrace();
        //        if(stackTrace.GetFrame(1).GetMethod().Name == "UpdateModel")
        //        {
        //            _PID_current_loop = value;
        //            OnPropertyChanged();
        //        }
        //        else if(stackTrace.GetFrame(1).GetMethod().Name != "UpdateModel")
        //        {
        //            Rs232Interface.GetInstance.SendToParser(new PacketFields
        //            {
        //                Data2Send = value ? 1 : 0,
        //                ID = Convert.ToInt16(81),
        //                SubID = Convert.ToInt16(10),
        //                IsSet = true,
        //                IsFloat = false
        //            });
        //        }
        //    }
        //}
        //private bool _PID_speed_loop = false;

        //public bool PID_speed_loop
        //{
        //    get
        //    {
        //        return _PID_speed_loop;
        //    }
        //    set
        //    {
        //        if(!LeftPanelViewModel._app_running)
        //            return;
        //        // get call stack
        //        StackTrace stackTrace = new StackTrace();
        //        if(stackTrace.GetFrame(1).GetMethod().Name == "UpdateModel")
        //        {
        //            _PID_speed_loop = value;
        //            OnPropertyChanged();
        //        }
        //        else if(stackTrace.GetFrame(1).GetMethod().Name != "UpdateModel")
        //        {
        //            Rs232Interface.GetInstance.SendToParser(new PacketFields
        //            {
        //                Data2Send = value ? 1 : 0,
        //                ID = Convert.ToInt16(82),
        //                SubID = Convert.ToInt16(10),
        //                IsSet = true,
        //                IsFloat = false
        //            });
        //        }
        //    }
        //}
        //private bool _PID_position_loop = false;

        //public bool PID_position_loop
        //{
        //    get
        //    {
        //        return _PID_position_loop;
        //    }
        //    set
        //    {
        //        if(!LeftPanelViewModel._app_running)
        //            return;
        //        // get call stack
        //        StackTrace stackTrace = new StackTrace();
        //        if(stackTrace.GetFrame(1).GetMethod().Name == "UpdateModel")
        //        {
        //            _PID_position_loop = value;
        //            OnPropertyChanged();
        //        }
        //        else if(stackTrace.GetFrame(1).GetMethod().Name != "UpdateModel")
        //        {
        //            Rs232Interface.GetInstance.SendToParser(new PacketFields
        //            {
        //                Data2Send = value ? 1 : 0,
        //                ID = Convert.ToInt16(83),
        //                SubID = Convert.ToInt16(10),
        //                IsSet = true,
        //                IsFloat = false
        //            });
        //        }
        //    }
        //}
        #endregion PID
    }
}
