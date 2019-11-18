using System;
using System.Collections.ObjectModel;
using SuperButton.CommandsDB;
using SuperButton.Models.DriverBlock;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using SuperButton.Common;
using SuperButton.Helpers;
using System.Linq;
using SuperButton.Views;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Abt.Controls.SciChart;


namespace SuperButton.ViewModels
{
    class IOViewModel : ViewModelBase
    {
        private static readonly object Synlock = new object();
        private static IOViewModel _instance;
        public static IOViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new IOViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        private IOViewModel()
        {
        }

        private ObservableCollection<object> _analogCommandList;
        public ObservableCollection<object> AnalogCommandList
        {
            get
            {
                return Commands.GetInstance.DataCommandsListbySubGroup["AnalogCommand List"];
            }
            set
            {
                _analogCommandList = value;
                OnPropertyChanged();
            }
        }
    }
}
