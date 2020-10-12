using System;
using System.Windows.Input;
using MotorController.Models;
using MotorController.Models.DriverBlock;
using MathNet.Numerics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Linq;
using MotorController.CommandsDB;

namespace MotorController.ViewModels
{
    public class BoolViewIndModel : ViewModelBase
    {
        private readonly BaseModel _baseModel = new BaseModel();

        public string CommandName { get { return _baseModel.CommandName; } set { _baseModel.CommandName = value; } }

        private int _commandValue = 0;
        public int CommandValue { get { return _commandValue;  } set { _commandValue = value; OnPropertyChanged(); }  }
        public string CommandId { get { return _baseModel.CommandID; } set { _baseModel.CommandID = value; } }

        public string CommandSubId { get { return _baseModel.CommandSubID; } set { _baseModel.CommandSubID = value; } }

        public bool IsFloat { get { return _baseModel.IsFloat; } set { _baseModel.IsFloat = value; } }
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
