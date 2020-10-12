using Abt.Controls.SciChart;
using MotorController.Models;
using MotorController.Models.DriverBlock;
using MotorController.Models.ParserBlock;
using MotorController.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MotorController.ViewModels
{
    public class UC_ChannelViewModel : ViewModelBase
    {
        private string _label = "", _gain = "";
        private short _command_id = 0, _command_subid = 0;
        private bool _isEnbaled = true;
        private string _y_axis_title;
        private SolidColorBrush _chBackground = new SolidColorBrush(Colors.Transparent);

        public string Label { get { return _label; } set { _label = value; OnPropertyChanged(); } }
        public short CommandId { get { return _command_id; } set { _command_id = value; OnPropertyChanged(); } }
        public short CommandSubId { get { return _command_subid; } set { _command_subid = value; OnPropertyChanged(); } }
        public bool IsEnabled { get { return _isEnbaled; } set { _isEnbaled = value; OnPropertyChanged(); } }
        public SolidColorBrush ChBackground { get { return _chBackground; } set { _chBackground = value; OnPropertyChanged(); } }
        public string Gain { get { return _gain; } set { _gain = value; OnPropertyChanged(); } }
        public string Y_Axis_Title { get { return _y_axis_title; } set { _y_axis_title = value; OnPropertyChanged(); } }

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
        #region ICommand
        public ICommand ChSelectionChanged
        {
            get { return new RelayCommand(Send_Plot1); }
        }
        public ICommand ChComboDropDownOpened
        {
            get { return new RelayCommand(ComboDropDownOpenedFunc); }
        }
        public ICommand ChComboDropDownClosed
        {
            get { return new RelayCommand(ComboDropDownClosedFunc); }
        }
        #endregion ICommand
        private static bool _isOpened = false;
        private void ComboDropDownOpenedFunc()
        {
            _isOpened = true;
        }
        private void ComboDropDownClosedFunc()
        {
            _isOpened = false;
        }
        private void Send_Plot1()
        {
            ChSelectedItem = ChItemsSource.ElementAt(ChSelectedIndex);
        }
        public ObservableCollection<string> _itemsSource = new ObservableCollection<string>();
        private string _selectedItem;
        private int _chSelectedIndex = 0, _ch2Index = 0;

        public int ChSelectedIndex
        {
            get { return _chSelectedIndex; }
            set
            {
                _chSelectedIndex = value;
                if(value > 0 && ChSelectedItem != null)
                    Y_Axis_Title = "CH " + CommandSubId.ToString() + ": " + OscilloscopeViewModel.GetInstance.ChannelYtitles.First(x => x.Key == ChSelectedItem).Value;
                else
                    Y_Axis_Title = "";
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> ChItemsSource
        {
            get
            {
                if(_itemsSource == null)
                    _itemsSource = new ObservableCollection<string>();
                return _itemsSource;
            }
            set
            {
                _itemsSource = value;
                OnPropertyChanged();
            }
        }
        public string ChSelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                StackTrace stackTrace = new StackTrace();

                //if(!LeftPanelViewModel.GetInstance.StarterOperationFlag)
                {
                    lock(ParserRayonM1.PlotListLock)
                    {
                        OscilloscopeViewModel.GetInstance.ChannelsYaxeMerge(_itemsSource.IndexOf(_selectedItem), CommandSubId);

                        if(Rs232Interface.GetInstance.IsSynced)
                        {
                            PacketFields RxPacket;
                            RxPacket.ID = CommandId;
                            RxPacket.IsFloat = false;
                            RxPacket.IsSet = true;
                            RxPacket.SubID = CommandSubId;
                            RxPacket.Data2Send = _itemsSource.IndexOf(_selectedItem);

                            Rs232Interface.GetInstance.SendToParser(RxPacket);
                            OscilloscopeViewModel.GetInstance.ChannelsplotActivationMerge();
                        }
                    }
                }
                //update step
                OscilloscopeViewModel.GetInstance.StepRecalcMerge();
                //update y axes
                //OscilloscopeViewModel.GetInstance.ChannelYtitles.TryGetValue(value, out "");
                //OscilloscopeViewModel.GetInstance.YaxeTitle = _ch1Title == _ch2Title ? _ch1Title : "";

            }
        }
    }
}
