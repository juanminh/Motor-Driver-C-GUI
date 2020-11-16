using MotorController.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MotorController.ViewModels
{
    public class UC_ProgrammerTextBoxViewModel : ViewModelBase
    {
        private string _textStr = "";
        public string TextStr
        {
            get { return _textStr; }
            set
            {
                if(HexadecimalCheck)
                    value = value.ToUpper();
                _textStr = value;
                OnPropertyChanged();
            }
        }
        private bool _isReadOnly ;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                OnPropertyChanged();
            }
        }
        private System.Text.RegularExpressions.Regex _regEx = new System.Text.RegularExpressions.Regex("^[0-9.-]+$");
        public System.Text.RegularExpressions.Regex RegEx
        {
            get
            {
                return _regEx;
            }
            set
            {
                _regEx = value;
                OnPropertyChanged();
            }
        }

        private bool _decimalCheck = true;
        public bool DecimalCheck
        {
            get
            {
                return _decimalCheck;
            }
            set
            {
                StackTrace _stck = new StackTrace();
                if(_stck.GetFrame(1).GetMethod().Name != "set_HexadecimalCheck")
                {
                    if(!value)
                        return;
                    RegEx = new System.Text.RegularExpressions.Regex("^[0-9.-]+$");
                    TextStr = int.Parse(TextStr, System.Globalization.NumberStyles.HexNumber).ToString();
                    _decimalCheck = value;
                    HexadecimalCheck = !value;
                }
                else
                {
                    _decimalCheck = value;
                }
                OnPropertyChanged();
            }
        }
        string ToHexString(int f)
        {
            var bytes = BitConverter.GetBytes(f);
            var i = BitConverter.ToInt32(bytes, 0);
            return i.ToString("X");
        }
        private bool _hexadecimalCheck = false;
        public bool HexadecimalCheck
        {
            get
            {
                return _hexadecimalCheck;
            }
            set
            {
                StackTrace _stck = new StackTrace();
                if(_stck.GetFrame(1).GetMethod().Name != "set_DecimalCheck")
                {
                    if(!value)
                        return;
                    RegEx = new System.Text.RegularExpressions.Regex("^[0-9A-Fa-f]*$");
                    if(TextStr.Contains("."))
                        return;
                    int number = 0;
                    bool isInputInteger = int.TryParse(TextStr, out number) || String.IsNullOrEmpty(TextStr);
                    TextStr = isInputInteger ? ToHexString(number) : TextStr;
                    _hexadecimalCheck = isInputInteger ? value : _hexadecimalCheck;
                    DecimalCheck = isInputInteger ? !value : value;
                }
                else
                {
                    _hexadecimalCheck = value;
                }
                OnPropertyChanged();
            }
        }
    }
}
