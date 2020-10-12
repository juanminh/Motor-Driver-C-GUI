using MotorController.CommandsDB;
using System.Collections.ObjectModel;

namespace MotorController.ViewModels
{
    public class RightPanelViewModel : ViewModelBase
    {

        public RightPanelViewModel()
        {
            //Commands.GetInstance.GenerateRPCommands();
        }
        private ObservableCollection<object> _rpCommandsList;
        public ObservableCollection<object> RPCommandsList
        {

            get
            {
                return Commands.GetInstance.GenericCommandsGroup["RPCommands List"]; //Motion Limit
            }
            set
            {
                _rpCommandsList = value;
                OnPropertyChanged();
            }


        }
    }
}
