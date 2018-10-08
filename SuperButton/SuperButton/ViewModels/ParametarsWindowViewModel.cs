﻿using System.Collections.ObjectModel;
using SuperButton.CommandsDB;
using System.Windows.Input;
using SuperButton.Models;
using System.Windows.Controls;
using System;
using System.Threading;

namespace SuperButton.ViewModels
{
    internal class ParametarsWindowViewModel : ViewModelBase
    {
        private bool _isHallEnabled;
        private bool _isQep1Enabled;
        private bool _isQep2Enabled;
        private bool _isSsiFeedbackEnabled;
        private bool _isDigitalbackEnabled;
        private bool _isAnalogbackEnabled;
        private OperationViewModel _operationViewModel;
        private CalibrationViewModel _calibrationViewModel;
        private MotionViewModel _motionViewModel;
        private MaintenanceViewModel _maintenanceViewModel;
        

        public ParametarsWindowViewModel()
        {
            _operationViewModel = OperationViewModel.GetInstance;
            _calibrationViewModel = CalibrationViewModel.GetInstance;
            _motionViewModel = MotionViewModel.GetInstance;
            _maintenanceViewModel = MaintenanceViewModel.GetInstance;
        }
            
        public ObservableCollection<object> ControlList
        {

            get
            {
                return Commands.GetInstance.EnumCommandsListbySubGroup["Control"];
            }


        }

        public ObservableCollection<object> MotorlList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Motor"];
            }

        }
        private ObservableCollection<object> _motorLimitlList;
        public ObservableCollection<object> MotorLimitlList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Motion Limit"];
            }
            set
            {
                _motorLimitlList = value;
                OnPropertyChanged();
            }

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

        private ObservableCollection<object> _qep1FeedBackList;
        private ObservableCollection<object> _qep1FdBckList;
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
        public ObservableCollection<object> Qep2FeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Qep2"];
            }

        }

        public ObservableCollection<object> SsiFeedbackFeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["SSI_Feedback"];
            }

        }
        public ObservableCollection<object> DigitalFeedbackFeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Digital"];
            }

        }

        public ObservableCollection<object> AnalogFeedbackFeedBackList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["Analog"];
            }

        }

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
        private string _motorDriver;

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

       // private ObservableCollection<object> _deviceSerialList;
        public ObservableCollection<object> DeviceSerialList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["DeviceSerial"];
            }

        }
      //  private ObservableCollection<object> _driverFullScaleList;
        public ObservableCollection<object> DriverFullScaleList
        {

            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["DriverFullScale"];
            }

        }

        #region enables

        public bool IsHallFeedBackEnabled
        {

            get { return _isHallEnabled; }

            set
            {
                _isHallEnabled = value;
                OnPropertyChanged();

            }
        }

        public bool IsQep1FeedBackEnabled
        {

            get
            {
                return _isQep1Enabled;
            }

            set
            {
                _isQep1Enabled = value;
                OnPropertyChanged();

            }
        }
        public bool IsQep2FeedBackEnabled
        {

            get
            {
                return _isQep2Enabled;
            }

            set
            {
                _isQep2Enabled = value;
                OnPropertyChanged();

            }
        }
        public bool IsSsiFeedbackEnabled
        {

            get
            {
                return _isSsiFeedbackEnabled;
            }

            set
            {
                _isSsiFeedbackEnabled = value;
                OnPropertyChanged();

            }
        }
        public bool IsDigitalFeedbackEnabled
        {

            get
            {
                return _isDigitalbackEnabled;
            }

            set
            {
                _isDigitalbackEnabled = value;
                OnPropertyChanged();

            }
        }
        public bool IsAnalogFeddbackEnabled
        {

            get
            {
                return _isAnalogbackEnabled;
            }

            set
            {
                _isAnalogbackEnabled = value;
                OnPropertyChanged();

            }
        }

        #endregion


        public virtual ICommand TestEnumChange
        {
            get
            {


                return new RelayCommand(EnumChange, CheckValue);
            }
        }

        public virtual ICommand SelectedItemChangedCommand
        {
            get
            {


                return new RelayCommand(CheckBox, CheckValue);
            }
        }
        public virtual ICommand ChangeChekBox
        {
            get
            {


                return new RelayCommand(changechekboxvalue, CheckValue);
            }
        }

        public OperationViewModel OperationViewModel
        {
            get { return _operationViewModel; }
        }

        public CalibrationViewModel CalibrationViewModel
        {
            get { return _calibrationViewModel; }
        }

        public MotionViewModel MotionViewModel
        {
            get { return _motionViewModel; }
        }

        public MaintenanceViewModel MaintenanceViewModel
        {
            get { return _maintenanceViewModel; }
        }
        private void changechekboxvalue()
        {
            IsHallFeedBackEnabled = !IsHallFeedBackEnabled;
        }



        private void CheckBox()
        {

        }

        private bool CheckValue()
        {
            return true;
        }

        private void EnumChange()
        {

            //EnumViewModel tmp = (EnumViewModel)CommandsDB.Commands.GetInstance.EnumViewCommandsList[new Tuple<int, int>(212, 0)];
            //tmp.IsUpdate = true;
            //tmp.SelectedValue = "Current Control";
            //tmp.IsUpdate = false;
        }

        

        //public string MotorDriver
        //{
        //    get
        //    {
        //        return _motorDriver;
        //    }

        //    set
        //    {
        //        _motorDriver = value;
        //        OnPropertyChanged("MotorDriver"); //"DriverVersion"
        //    }
        //}

    }
}
