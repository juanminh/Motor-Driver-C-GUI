using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorController.Common
{
    public class Consts
    {
        #region CONSTS
        public const int BOOL_IDLE = 0;
        public const int BOOL_PASSED = 1;
        public const int BOOL_FAILED = 2;
        public const int BOOL_DISABLED = 3;
        public const int BOOL_RUNNING = 4;
        public const int BOOL_STOPPED = 5;
        public const int BOOL_DISC_OFF = 6;
        public const int BOOL_DISC_ON = 7;

        public const int INTERFACE_RS232 = 1;
        public const int CAN_DRIVER_KVASER = 0;


        public const int KEY_HISTORY_DIR = 1;
        public const int KEY_PARAMS_DIR = 2;
        

        public static Dictionary<int, string> dic_paths = new Dictionary<int, string>()
                                                                   {
                                                                       {KEY_HISTORY_DIR,Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MotorController\\SuperMotorController_Params"},
                                                                       {KEY_PARAMS_DIR, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MotorController\\SuperMotorControllerHistory"}
                                                                   };
        public static string Version = "Ver. 1.3.19";
        public enum eProject
        {
            REDLER = 0,
            STXI = 1
        };
        public static eProject _project = eProject.STXI;
        public enum eBuild
        {
            DEBUG = 0,
            RELEASE = 1
        };
        public static eBuild _build = eBuild.DEBUG;
        #endregion


    }
}