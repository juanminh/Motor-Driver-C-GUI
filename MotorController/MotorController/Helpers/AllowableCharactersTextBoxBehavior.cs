using MotorController.ViewModels;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Globalization;

namespace MotorController.Helpers
{
    public class AllowableCharactersTextBoxBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(AssociatedObject, OnPaste);
        }
        TextBox _tb_call;
        void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            _tb_call = sender as TextBox;
            
            e.Handled = !IsValid(e.Text, false);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
            DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if(e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = Convert.ToString(e.DataObject.GetData(DataFormats.Text));

                if(!IsValid(text, true))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        private bool IsValid(string newText, bool paste)
        {
            switch(newText)
            {
                case ".":
                    if(this.AssociatedObject.Text.Contains(newText) || this.AssociatedObject.Text.Length == 0)
                        return false;
                    break;
                case "-":
                    if(_tb_call.SelectedText == _tb_call.Text && Regex.IsMatch(newText, RegularExpression))
                        return true;
                        break;
            }
            
            return !ExceedsMaxLength(newText, paste) && Regex.IsMatch(newText, RegularExpression);
        }

        private bool ExceedsMaxLength(string newText, bool paste)
        {
            if(MaxLength == 0)
                return false;

            return LengthOfModifiedText(newText, paste) > MaxLength;
        }

        private int LengthOfModifiedText(string newText, bool paste)
        {
            var countOfSelectedChars = this.AssociatedObject.SelectedText.Length;
            var caretIndex = this.AssociatedObject.CaretIndex;
            string text = this.AssociatedObject.Text;

            if(newText == "-" || newText == ".")
            {
                if(this.AssociatedObject.Text.Contains(newText))
                    return MaxLength + 1; // to get a false value in "IsValid" function
                else if(newText == "-" && text.Length > 0 && caretIndex > 0)
                    return MaxLength + 1; // to get a false value in "IsValid" function
            }

            if(countOfSelectedChars > 0 || paste)
            {
                text = text.Remove(caretIndex, countOfSelectedChars);
                return text.Length + newText.Length;
            }
            else
            {
                var insert = Keyboard.IsKeyToggled(Key.Insert);

                return insert && caretIndex < text.Length ? text.Length : text.Length + newText.Length;
            }
        }
        public static readonly DependencyProperty RegularExpressionProperty =
             DependencyProperty.Register("RegularExpression", typeof(string), typeof(AllowableCharactersTextBoxBehavior),
             new FrameworkPropertyMetadata(".*"));
        public string RegularExpression
        {
            get
            {
                return (string)base.GetValue(RegularExpressionProperty);
            }
            set
            {
                base.SetValue(RegularExpressionProperty, value);
            }
        }

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int), typeof(AllowableCharactersTextBoxBehavior),
            new FrameworkPropertyMetadata(int.MinValue));
        public int MaxLength
        {
            get
            {
                return (int)base.GetValue(MaxLengthProperty);
            }
            set
            {
                base.SetValue(MaxLengthProperty, value);
            }
        }
    }
}
