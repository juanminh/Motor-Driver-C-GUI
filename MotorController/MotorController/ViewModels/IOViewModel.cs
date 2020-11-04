using System.Collections.ObjectModel;
using MotorController.Common;

namespace MotorController.ViewModels
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
                return Commands.GetInstance.GenericCommandsGroup["AnalogCommand List"];
            }
            set
            {
                _analogCommandList = value;
                OnPropertyChanged();
            }
        }
    }
}
