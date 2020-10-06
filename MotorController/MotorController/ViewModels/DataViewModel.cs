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
using MotorController.ViewModels;

namespace MotorController.ViewModels
{
    public class DataViewModel : ViewModelBase
    {

        private SolidColorBrush _background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")); // Gray
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
                _baseModel.CommandValue = value;
                GetCount = -1;
                OnPropertyChanged();
            }
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
                _isSelected = value;
                OnPropertyChanged();
            }
        }
        private SolidColorBrush _backgroundStdby = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333")); // Gray
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
        private double _fontSize = 13.33;
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
        private void BuildPacketTosend()
        {
            if(LeftPanelViewModel.GetInstance.ConnectButtonContent == "Disconnect")
            {
                if(CommandValue != "")
                {
                    var tmp = new PacketFields
                    {
                        Data2Send = CommandValue,
                        ID = Convert.ToInt16(CommandId),
                        SubID = Convert.ToInt16(CommandSubId),
                        IsSet = true,
                        IsFloat = IsFloat,
                    };
                    Task.Factory.StartNew(action: () => { Rs232Interface.GetInstance.SendToParser(tmp); });
                }
                Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))].IsSelected = false;
                Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))].Background = _background;
            }
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
                foreach(var list in Commands.GetInstance.DataViewCommandsList)
                {
                    try
                    {
                        Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].IsSelected = false;
                        Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].Background = _background;
                    }
                    catch(Exception)
                    {
                    }
                }
                Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))].IsSelected = true;
                Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))].Background = _backgroundSelected;
            }
        }
        private void MouseLeaveCommandFunc()
        {
            foreach(var list in Commands.GetInstance.DataViewCommandsList)
            {
                try
                {
                    Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].IsSelected = false;
                    Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(list.Value.CommandId), Convert.ToInt16(list.Value.CommandSubId))].Background = _background;
                }
                catch(Exception)
                {

                }
            }
        }

        private SolidColorBrush _foreground = new SolidColorBrush(Colors.White);
        public SolidColorBrush Foreground
        {
            get
            {
                return _foreground;
            }
            set
            {
                _foreground = value;
                OnPropertyChanged();
            }
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
        //public void _changeForeGround()
        //{
        //    if(GetCount != -1)
        //        Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))].Foreground = _foreGroundNotRefreshed;
        //    else
        //        Commands.GetInstance.DataViewCommandsList[new Tuple<int, int>(Convert.ToInt16(CommandId), Convert.ToInt16(CommandSubId))].Foreground = _foreGroundRefreshed;
        //}
    }
}