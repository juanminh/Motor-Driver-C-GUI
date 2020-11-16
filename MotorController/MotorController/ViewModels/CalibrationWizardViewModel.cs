﻿using MotorController.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorController.ViewModels
{
    public class CalibrationWizardViewModel : ViewModelBase
    {
        //public CalibrationWizardViewModel()
        //{
        //}
        //~CalibrationWizardViewModel() { }
        private bool _isWizard = true;
        public bool isWizard
        {
            get { return _isWizard; }
            set
            {
                _isWizard = value;
                OnPropertyChanged("isWizard");
            }
        }
        private int _calibStatus;
        public int CalibStatus {
            get { return _calibStatus; }
            set {
                _calibStatus = value;
                OnPropertyChanged("CalibStatus");
            }
        }
        private int _calibTimeout;
        public int CalibTimeout
        {
            get { return _calibTimeout; }
            set
            {
                _calibTimeout = value;
                OnPropertyChanged("CalibTimeout");
            }
        }

        private bool _advanceMode_Calibration = Consts._build == Consts.eBuild.DEBUG ? true :false;
        public bool AdvanceMode_Calibration {
            get { return _advanceMode_Calibration; }
            set
            {
                _advanceMode_Calibration = value;
                OnPropertyChanged("AdvanceMode_Calibration");
            }
        }
        private string _calibrationName;
        public string CalibrationName
        {
            get { return _calibrationName; }
            set
            {
                _calibrationName = value;
                OnPropertyChanged("CalibrationName");
            }
        }
        private bool _calibrationPerform = true;
        public bool CalibrationPerform
        {
            get { return _calibrationPerform; }
            set
            {
                _calibrationPerform = value;
                OnPropertyChanged("CalibrationPerform");
            }
        }
        private bool _calibrationEnabled = true;
        public bool CalibrationEnabled
        {
            get { return _calibrationEnabled; }
            set
            {
                _calibrationEnabled = value;
                OnPropertyChanged("CalibrationEnabled");
            }
        }
        private string _commandValue = "";
        public string CommandValue { get { return _commandValue; } set { _commandValue = value; OnPropertyChanged(); } }
        private string _commandId = "";
        public string CommandId { get { return _commandId; } set { _commandId = value; } }
        private string _commandSubId = "";
        public string CommandSubId { get { return _commandSubId; } set { _commandSubId = value; } }
        private int _getCount = -1;
        public int GetCount
        {
            get { return _getCount; }
            set
            {
                _getCount = value;
                if(_getCount >= 1)
                    GetCount_bool = true;
                else
                    GetCount_bool = false;

                OnPropertyChanged();
            }
        }
        private bool _getCount_bool = false;
        public bool GetCount_bool
        {
            get { return _getCount_bool; }
            set
            {
                _getCount_bool = value;
                OnPropertyChanged();
            }
        }
    }
}
