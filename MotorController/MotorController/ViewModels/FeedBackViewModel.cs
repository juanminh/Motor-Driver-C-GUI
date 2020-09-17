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
    internal class FeedBackViewModel : ViewModelBase
    {
        private static readonly object Synlock = new object();
        private static FeedBackViewModel _instance;

        public static FeedBackViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new FeedBackViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }

        }
        private FeedBackViewModel()
        {

        }

        private ObservableCollection<object> _hallFeedBackList;
        public ObservableCollection<object> HallFeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Hall"];
            }
            set
            {
                _hallFeedBackList = value;
                OnPropertyChanged();
            }

        }

        //private bool _external_interpolation = false;

        //public bool External_interpolation
        //{
        //    get
        //    {
        //        return _external_interpolation;
        //    }
        //    set
        //    {
        //        if(!LeftPanelViewModel._app_running)
        //            return;
        //        // get call stack
        //        StackTrace stackTrace = new StackTrace();
        //        if(stackTrace.GetFrame(1).GetMethod().Name == "UpdateModel")
        //        {
        //            _external_interpolation = value;
        //            OnPropertyChanged();
        //        }
        //        else if(stackTrace.GetFrame(1).GetMethod().Name != "UpdateModel")
        //        {
        //            Rs232Interface.GetInstance.SendToParser(new PacketFields
        //            {
        //                Data2Send = value ? 1 : 0,
        //                ID = Convert.ToInt16(78),
        //                SubID = Convert.ToInt16(1),
        //                IsSet = true,
        //                IsFloat = false
        //            });
        //        }
        //    }
        //}

        private ObservableCollection<object> _externalInterpolation;
        public ObservableCollection<object> ExternalInterpolation
        {

            get
            {
                return Commands.GetInstance.ToggleSwitchList["Feedback Sync"];
            }
            set
            {
                _externalInterpolation = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<object> _feedbackSyncList;
        public ObservableCollection<object> FeedbackSyncList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["FeedbackSync"];
            }
            set
            {
                _feedbackSyncList = value;
                OnPropertyChanged();
            }

        }

        private ObservableCollection<object> _qep1FeedBackList;
        private ObservableCollection<object> _qep2FeedBackList;
        private ObservableCollection<object> _qep1FdBckList;
        private ObservableCollection<object> _qep2FdBckList;

        public ObservableCollection<object> Qep1FeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Qep1"];
            }
            set
            {
                _qep1FeedBackList = value;
                OnPropertyChanged();
            }

        }
        public ObservableCollection<object> Qep1FdBckList
        {

            get
            {
                return Commands.GetInstance.EnumCommandsListbySubGroup["Qep1Bis"];
            }
            set
            {
                _qep1FdBckList = value;
                OnPropertyChanged();
            }

        }
        public ObservableCollection<object> Qep2FdBckList
        {

            get
            {
                return Commands.GetInstance.EnumCommandsListbySubGroup["Qep2Bis"];
            }
            set
            {
                _qep2FdBckList = value;
                OnPropertyChanged();
            }

        }

        public ObservableCollection<object> Qep2FeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Qep2"];
            }
            set
            {
                _qep2FeedBackList = value;
                OnPropertyChanged();
            }

        }

        public ObservableCollection<object> SsiFeedbackFeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["SSI_Feedback"];
            }

        }
    }
}
