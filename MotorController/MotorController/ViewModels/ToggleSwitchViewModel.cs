using MotorController.CommandsDB;
using MotorController.Models;
using MotorController.Models.DriverBlock;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MotorController.ViewModels
{
    public class ToggleSwitchViewModel : ViewModelBase
    {

        private SolidColorBrush _backgroundSmallFontSelected = new SolidColorBrush(Colors.Gray);

        private string _label = "";
        private short _command_id = 0, _command_subid = 0;
        private bool _is_checked = false;

        public string Label { get { return _label; } set { _label = value; OnPropertyChanged(); } }
        public short CommandId { get { return _command_id; } set { _command_id = value; OnPropertyChanged(); } }
        public short CommandSubId { get { return _command_subid; } set { _command_subid = value; OnPropertyChanged(); } }
        public bool IsChecked
        {
            get { return _is_checked; }
            set
            {
                if(!LeftPanelViewModel._app_running)
                    return;
                StackTrace stackTrace = new StackTrace();
                if(stackTrace.GetFrame(1).GetMethod().Name == "UpdateModel")
                {
                    CheckedBackground = CheckedBackground_final;
                    _is_checked = value;
                }
                else
                {
                    //CheckedBackground = new SolidColorBrush(Colors.Gray);
                    BuildPacketTosend();
                }
                OnPropertyChanged();
            }
        }

        private void BuildPacketTosend()
        {
            if(LeftPanelViewModel.GetInstance.ConnectButtonContent == "Disconnect")
            {
                var tmp = new PacketFields
                {
                    Data2Send = !IsChecked ? 1 : 0,
                    ID = CommandId,
                    SubID = CommandSubId,
                    IsSet = true,
                    IsFloat = false,
                };
                Task.Factory.StartNew(action: () => { Rs232Interface.GetInstance.SendToParser(tmp); });
            }
        }

        private SolidColorBrush _checkedBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a5ed5"));
        private SolidColorBrush _checkedBackground_final = new SolidColorBrush(Colors.Red);
        private SolidColorBrush _uncheckedBackground = new SolidColorBrush(Colors.Gray);
        private string _checkedText = "On";
        private string _uncheckedText = "Off";
        public SolidColorBrush CheckedBackground_final
        {
            get
            {
                return _checkedBackground_final;
            }
            set
            {
                _checkedBackground_final = value;
                OnPropertyChanged();
            }
        }
        public SolidColorBrush CheckedBackground
        {
            get
            {
                return _checkedBackground;
            }
            set
            {
                _checkedBackground = value;
                OnPropertyChanged();
            }
        }
        public SolidColorBrush UnCheckedBackground
        {
            get
            {
                return _uncheckedBackground;
            }
            set
            {
                _uncheckedBackground = value;
                OnPropertyChanged();
            }
        }
        public string CheckedText { get { return _checkedText; } set { _checkedText = value; OnPropertyChanged(); } }
        public string UnCheckedText { get { return _uncheckedText; } set { _uncheckedText = value; OnPropertyChanged(); } }

        //public SolidColorBrush BackgroundSmallFont
        //{
        //    get
        //    {
        //        if(!IsSelected)
        //            return new SolidColorBrush(Colors.Gray);
        //        else
        //            return new SolidColorBrush(Colors.Red);
        //    }
        //    set
        //    {
        //        if(IsSelected)
        //            _baseModel.Background = new SolidColorBrush(Colors.Red);
        //        else
        //            _baseModel.Background = new SolidColorBrush(Colors.Gray);

        //        OnPropertyChanged("BackgroundSmallFont");
        //    }

        //}
        //public SolidColorBrush BackgroundStd
        //{
        //    get
        //    {
        //        if(!IsSelected)
        //            return new SolidColorBrush(Colors.White);
        //        else
        //            return new SolidColorBrush(Colors.Red);
        //    }
        //    set
        //    {
        //        if(IsSelected)
        //            _baseModel.Background = new SolidColorBrush(Colors.Red);
        //        else
        //            _baseModel.Background = new SolidColorBrush(Colors.White);

        //        OnPropertyChanged("BackgroundStd");
        //    }

        //}
    }
}
