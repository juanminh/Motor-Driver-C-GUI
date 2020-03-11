using System;

namespace SuperButton
{
    public partial class WizardWindowViewModel
    {
        private void saveWizardParams()
        {

            string path = "\\MotorController\\Wizard\\";
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + path;
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string _file_name = "WizardParameters.ini";
            iniFile _wizard_parameters_file = new iniFile(path + _file_name);


            string _section = "Wizard";

            #region Save_Parameters
            _wizard_parameters_file.Write("Motor Type", MotorType, _section);

            #endregion Save_Parameters
            #region Advanced_Configuration

            #endregion Advanced_Configuration
            for(int i = 0; i < GetInstance.OperationList.Count; i++)
            {
                if(GetInstance.OperationList.ElementAt(i).Value.CommandName != "Load Default" &&
                   GetInstance.OperationList.ElementAt(i).Value.CommandName != "Save" &&
                   GetInstance.OperationList.ElementAt(i).Value.CommandName != "Reset" &&
                   GetInstance.OperationList.ElementAt(i).Value.CommandName != "Synchronisation Command")
                    _wizard_parameters_file.Write(GetInstance.OperationList.ElementAt(i).Value.CommandName, GetInstance.OperationList.ElementAt(i).Value.CommandValue, _section);
            }
            for(int i = 0; i < GetInstance.OperationList.Count; i++)
            {
                if(GetInstance.OperationList.ElementAt(i).Value.CommandName != "Load Default" &&
                    GetInstance.OperationList.ElementAt(i).Value.CommandName != "Save" &&
                    GetInstance.OperationList.ElementAt(i).Value.CommandName != "Reset" &&
                    GetInstance.OperationList.ElementAt(i).Value.CommandName != "Synchronisation Command")
                    Debug.WriteLine(GetInstance.OperationList.ElementAt(i).Value.CommandName + ": " + _wizard_parameters_file.Read(GetInstance.OperationList.ElementAt(i).Value.CommandName, _section));
            }
        }
    }
}
