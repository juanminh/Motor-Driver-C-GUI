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
using MotorController.Common;
using MotorController.ViewModels;
using System.Windows.Controls;

namespace MotorController.ViewModels
{
    public class DataViewModel : ViewModelBase
    {

        public static SolidColorBrush _background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")); // Gray  333333
        private SolidColorBrush _backgroundStdby = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")); // Gray

        private SolidColorBrush _backgroundSelected = new SolidColorBrush(Colors.Red);
        private SolidColorBrush _backgroundIndicator = new SolidColorBrush(Colors.Black);
        private SolidColorBrush _foreGroundNotRefreshed = new SolidColorBrush(Colors.Yellow);
        private SolidColorBrush _foreGroundRefreshed = new SolidColorBrush(Colors.White);

        ICommand _mouseLeftClickCommand;
        ICommand _mouseLeaveCommand;
        private SolidColorBrush _backgroundSmallFontSelected = new SolidColorBrush(Colors.Gray);

        private readonly BaseModel _baseModel = new BaseModel();
        public string CommandName { get { return _baseModel.CommandName; } set { _baseModel.CommandName = value; } }
        public string CommandValue
        {
            get
            {
                return _baseModel.CommandValue;
            }
            set
            {
                _baseModel.CommandValue = _special_commands(value);
                GetCount = -1;
                OnPropertyChanged();
            }
        }
        private string _special_commands(string _value)
        {
            if(LeftPanelViewModel._app_running)
            {
                if(CommandId == "62" && CommandSubId == "10")
                    return "0x" + int.Parse(_value).ToString("X8");
                else if(CommandId == "33" && CommandSubId == "1")
                    LeftPanelViewModel.GetInstance.DriverStat = RefreshManager.GetInstance.CalibrationGetError(_value);
            }
            return _value;
        }
        public string CommandId { get { return _baseModel.CommandID; } set { _baseModel.CommandID = value; } }
        public string CommandSubId { get { return _baseModel.CommandSubID; } set { _baseModel.CommandSubID = value; } }
        public bool IsFloat { get { return _baseModel.IsFloat; } set { _baseModel.IsFloat = value; } }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if(_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        public SolidColorBrush Background
        {
            get
            {
                if(ReadOnly)
                    return _backgroundIndicator;
                return _backgroundStdby;
            }
            set
            {
                _backgroundStdby = value;
                OnPropertyChanged("Background");
            }

        }
        private bool _isReadOnly = false;
        public bool ReadOnly
        {
            get { return _isReadOnly; }
            set { _isReadOnly = value; OnPropertyChanged("ReadOnly"); }
        }
        private double _fontSize = 13;
        public double FontSize
        {
            get { return _fontSize; }
            set { _fontSize = value; OnPropertyChanged("FontSize"); }
        }
        private bool _enableTextBox = true;
        public bool EnableTextBox { get { return _enableTextBox; } set { _enableTextBox = value; OnPropertyChanged("EnableTextBox"); } }


        public virtual ICommand SendData
        {
            get
            {
                return new RelayCommand(BuildPacketTosend);
            }
        }
        public virtual ICommand ResetValue
        {
            get
            {
                return _mouseLeaveCommand ?? (_mouseLeaveCommand = new RelayCommand(MouseLeaveCommandFunc));
            }
        }
        public virtual ICommand MouseDoubleClick
        {
            get
            {
                return new RelayCommand(MouseDoubleClick_Func);
            }
        }
        private void BuildPacketTosend(object sender)
        {
            if(LeftPanelViewModel.GetInstance.ConnectButtonContent == "Disconnect")
            {
                Int32 _data = 0;
                if(!IsFloat)
                    if(!Int32.TryParse(CommandValue, out _data))
                        return;

                if(CommandValue != "")
                {
                    var tmp = new PacketFields
                    {
                        Data2Send = IsFloat ? CommandValue : _data.ToString(),
                        ID = Convert.ToInt16(CommandId),
                        SubID = Convert.ToInt16(CommandSubId),
                        IsSet = true,
                        IsFloat = IsFloat,
                    };
                    Task.Factory.StartNew(action: () => { Rs232Interface.GetInstance.SendToParser(tmp); });
                }
                ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))]).IsSelected = false;
                ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))]).Background = _background;
            }
            var _tb = sender as TextBox;
            _tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
        public ICommand MouseLeftClickCommand
        {
            get
            {
                return _mouseLeftClickCommand ?? (_mouseLeftClickCommand = new RelayCommand(MouseLeftClickFunc));
            }
        }
        private void MouseLeftClickFunc()
        {
            if(ReadOnly)
                return;
            if(LeftPanelViewModel.GetInstance.ConnectButtonContent == "Disconnect")
            {
                foreach(var list in Commands.GetInstance.GenericCommandsList)
                {
                    try
                    {
                        if(list.Value.GetType().Name == "DataViewModel")
                        {
                            ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(((DataViewModel)list.Value).CommandId), Convert.ToInt16(((DataViewModel)list.Value).CommandSubId))]).IsSelected = false;
                            ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(((DataViewModel)list.Value).CommandId), Convert.ToInt16(((DataViewModel)list.Value).CommandSubId))]).Background = _background;
                        }
                    }
                    catch(Exception)
                    {
                    }
                }
                ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))]).IsSelected = true;
                ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))]).Background = _backgroundSelected;
            }
        }
        private void MouseLeaveCommandFunc(object sender)
        {
            foreach(var list in Commands.GetInstance.GenericCommandsList)
            {
                try
                {
                    if(list.Value.GetType().Name == "DataViewModel")
                    {
                        ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(((DataViewModel)list.Value).CommandId), Convert.ToInt16(((DataViewModel)list.Value).CommandSubId))]).IsSelected = false;
                        ((DataViewModel)Commands.GetInstance.GenericCommandsList[new Tuple<int, int>(Convert.ToInt16(((DataViewModel)list.Value).CommandId), Convert.ToInt16(((DataViewModel)list.Value).CommandSubId))]).Background = _background;
                    }
                }
                catch(Exception)
                {

                }
            }
            var _tb = sender as TextBox;
            _tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
        private void MouseDoubleClick_Func(object sender)
        {
            var _tb = sender as TextBox;
            _tb.SelectAll();
        }
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