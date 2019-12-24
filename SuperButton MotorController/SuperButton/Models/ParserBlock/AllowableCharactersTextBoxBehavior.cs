using SuperButton.ViewModels;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace SuperButton.Models
{
    class AllowableCharactersTextBoxBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(AssociatedObject, OnPaste);
        }
        void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsValid(e.Text, false);
        }

        //void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        //{
        //    var txt = sender as TextBox;
        //    string Name = txt.DataContext.ToString();
        //    string selection = txt.SelectedText;
        //    if (txt.Background.ToString() == "#FFFF0000" || Name.Contains(".ViewModels.NumericTextboxModel") || Name.Contains(".ViewModels.DebugObjModel")) {
        //        if (txt != null) e.Handled = !IsValid(e.Text, false, txt.Text, selection == "" ? false : true);
        //    }
        //    else
        //    {
        //        e.Handled = true;
        //    }
        //}

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= OnPreviewTextInput;
            DataObject.RemovePastingHandler(AssociatedObject, OnPaste);
        }
        
        //private void OnPaste(object sender, DataObjectPastingEventArgs e)
        //{
        //    var txt = sender as TextBox;
        //    if (e.DataObject.GetDataPresent(DataFormats.Text))
        //    {
        //        var text = Convert.ToString(e.DataObject.GetData(DataFormats.Text));
        //        string selection = txt.SelectedText;
        //        if (txt != null && !IsValid(text, true, txt.Text, selection == "" ? false : true))
        //        {
        //            e.CancelCommand();
        //        }
        //    }
        //    else
        //    {
        //        e.CancelCommand();
        //    }
        //}

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


        //private bool IsValid(string newText, bool paste, string currentvalue, bool TextSelected)
        //{
        //    if (newText != "\u001b")
        //    {
        //        int integ;
        //        if (currentvalue == "" && newText == "-") // start a negative number
        //            return true;
        //        if (newText == "." && currentvalue != "" && currentvalue != "-" && !currentvalue.Contains('.') && !TextSelected) // float number - adding dot
        //            return true;
        //        if (newText == "-" && currentvalue != "" && !currentvalue.Contains('-')) // && TextSelected delete all selected text and start a negative number
        //            return true;

        //        if (currentvalue.Contains('.') && newText != "-") // verify the number is valid (not a char)
        //        {
        //            if (Int32.TryParse(newText, out integ))
        //                return true;
        //            return false;
        //        }
        //        if (currentvalue.Contains('-') && newText != "-") // verify the number is valid for negative value
        //        {
        //            if (newText != ".")
        //            {
        //                if (Int32.TryParse(newText, out integ))
        //                    return true;
        //                return false;
        //            }
        //            else if (TextSelected && newText != ".") return true;
        //            else return false;
        //        }
        //        if (Int32.TryParse(newText, out integ))
        //            return true;


        //        return false;
        //    }
        //    else
        //    {
        //        currentvalue = "";
        //        return false;
        //    }
        //}

        private bool IsValid(string newText, bool paste)
        {
            switch(newText)
            {
                case ".":
                    if(this.AssociatedObject.Text.Contains(newText) || this.AssociatedObject.Text.Length == 0)
                        return false;
                    break;
                case "-":
                    if(this.AssociatedObject.Text.Length != 0)
                        return false;
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
            if(newText == "-" || newText == ".")
                if(this.AssociatedObject.Text.Contains(newText))
                    return MaxLength + 1;

            var countOfSelectedChars = this.AssociatedObject.SelectedText.Length;
            var caretIndex = this.AssociatedObject.CaretIndex;
            string text = this.AssociatedObject.Text;

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
