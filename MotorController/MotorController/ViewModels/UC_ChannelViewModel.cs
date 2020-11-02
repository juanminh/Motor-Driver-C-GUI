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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MotorController.ViewModels
{
    public class UC_ChannelViewModel : ViewModelBase
    {
        private string _label = "", _gain = "", _plot_type = "";
        private short _command_id = 0, _command_subid = 0;
        private bool _isEnbaled = false;
        private string _y_axis_title;
        private Color _chBackground = Colors.Transparent;
        private static bool _isOpened = false;

        public string Label { get { return _label; } set { _label = value; OnPropertyChanged(); } }
        public short CommandId { get { return _command_id; } set { _command_id = value; OnPropertyChanged(); } }
        public short CommandSubId { get { return _command_subid; } set { _command_subid = value; OnPropertyChanged(); } }
        public bool IsEnabled { get { return _isEnbaled; } set { _isEnbaled = value; OnPropertyChanged(); } }
        public Color ChBackground { get { return _chBackground; } set { _chBackground = value; OnPropertyChanged(); } }
        public string Gain { get { return _gain; } set { if(String.IsNullOrEmpty(value)) return; _gain = value; OnPropertyChanged(); } }
        public string Y_Axis_Title { get { return _y_axis_title; } set { _y_axis_title = value; OnPropertyChanged(); } }
        public string PlotType { get { return _plot_type; } set { _plot_type = value; OnPropertyChanged(); } }
        public bool IsOpened { get { return _isOpened; } set { _isOpened = value; OnPropertyChanged(); } }

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
            get { return new RelayCommand(Send_Plot, _is_opened); }
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
        private void ComboDropDownOpenedFunc()
        {
            this.IsOpened = true;
        }
        private void ComboDropDownClosedFunc()
        {
            this.IsOpened = false;
        }
        private bool _is_opened()
        {
            return this.IsOpened;
        }
        private void Send_Plot()
        {
            if(this.IsOpened)
                ChSelectedItem = ChItemsSource.ElementAt(ChSelectedIndex);
        }
        public ObservableCollection<string> _itemsSource = new ObservableCollection<string>();
        private string _selectedItem;
        private int _chSelectedIndex = 0;

        public int ChSelectedIndex
        {
            get { return _chSelectedIndex; }
            set
            {
                //Debug.WriteLine("Get - Ch: {0}, Index: {1} - isOpened: {2}", Label, value, this.IsOpened ? "true" : "false");
                GetCount = -1;
                if((_chSelectedIndex == value || IsOpened) && IsEnabled)
                {
                    OscilloscopeViewModel.GetInstance.StepRecalcMerge();
                    return;
                }
                _chSelectedIndex = value;
                /*if(value > 0)
                    Y_Axis_Title = "CH " + CommandSubId.ToString() + ": " + OscilloscopeViewModel.GetInstance.ChannelYtitles.Values.ElementAt(value);
                else
                    Y_Axis_Title = "";*/
                PlotType = value == 0 ? "" : OscilloscopeParameters.plotType_ls.ElementAt(value);
                if(value > 0)
                    Y_Axis_Title = OscilloscopeViewModel.GetInstance.ChannelYtitles.Values.ElementAt(value);
                IsEnabled = true;
                
                //OscilloscopeViewModel.GetInstance._update_channel_count(_chSelectedIndex, CommandSubId);

                OscilloscopeViewModel.GetInstance.ChannelsYaxeMerge(_chSelectedIndex, CommandSubId);
                OscilloscopeViewModel.GetInstance.ChannelsplotActivationMerge();
                OscilloscopeViewModel.GetInstance.StepRecalcMerge();

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
                if(!IsOpened)
                    return;
                _selectedItem = value;
                OnPropertyChanged();

                //Debug.WriteLine("Send - Ch: {0}, Index: {1} - isOpened: {2}", Label, value, this.IsOpened ? "true" : "false");

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
                OscilloscopeViewModel.GetInstance.StepRecalcMerge();
                IsOpened = false;
            }
        }

    }
}
