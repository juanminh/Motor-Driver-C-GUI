using MotorController.CommandsDB;
using MotorController.Models.DriverBlock;
using MotorController.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorController.ViewModels
{
    public class FilterViewModel : ViewModelBase
    {
        private static readonly object Synlock = new object();
        private static FilterViewModel _instance;

        public static FilterViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new FilterViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }

        }
        private FilterViewModel()
        {

        }
        #region Filter
        private ObservableCollection<object> _FilterList;
        public ObservableCollection<object> FilterList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["FilterList"];
            }
            set
            {
                _FilterList = value;
                OnPropertyChanged();
            }

        }
        //private bool _Filter_Enable = false;

        //public bool Filter_Enable
        //{
        //    get
        //    {
        //        return _Filter_Enable;
        //    }
        //    set
        //    {
        //        if(!LeftPanelViewModel._app_running)
        //            return;
        //        //_Filter_Enable = value;
        //        // get call stack
        //        StackTrace stackTrace = new StackTrace();
        //        if(stackTrace.GetFrame(1).GetMethod().Name == "UpdateModel")
        //        {
        //            _Filter_Enable = value;
        //            OnPropertyChanged("Filter_Enable");
        //        }
        //        else if(stackTrace.GetFrame(1).GetMethod().Name != "UpdateModel")
        //        {
        //            Rs232Interface.GetInstance.SendToParser(new PacketFields
        //            {
        //                Data2Send = value ? 1 : 0,
        //                ID = Convert.ToInt16(101),
        //                SubID = Convert.ToInt16(0),
        //                IsSet = true,
        //                IsFloat = false
        //            });
        //        }
        //    }
        //}
        
        private ObservableCollection<object> _Filter_Enable;
        public ObservableCollection<object> Filter_Enable
        {

            get
            {
                return Commands.GetInstance.ToggleSwitchList["Filter_Enable"];
            }
            set
            {
                _Filter_Enable = value;
                OnPropertyChanged();
            }
        }
        #endregion Filter
    }
}
