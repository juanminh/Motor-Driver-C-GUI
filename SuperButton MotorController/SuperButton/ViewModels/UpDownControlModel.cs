using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperButton.Models;
using Abt.Controls.SciChart;

namespace SuperButton.ViewModels
{
    public class UpDownControlModel : ViewModelBase
    {
        private string _data;
        public string Data
        {
            get { return _data; }
            set
            {
                if(value != "")
                {
                    if(Convert.ToInt16(value) > 999)
                        value = "999";
                    if(Convert.ToInt16(value) < 0)
                        value = "0";
                    _data = value;
                    OnPropertyChanged("Data");
                }
            }
        }

        public ActionCommand cmdUp { get { return new ActionCommand(Up); } }
        public ActionCommand cmdDown { get { return new ActionCommand(Down); } }

        private void Up()
        {
            Data = (Convert.ToInt16(_data) + 1).ToString();
        }
        private void Down()
        {
            Data = (Convert.ToInt16(_data) - 1).ToString();
        }
    }
}
