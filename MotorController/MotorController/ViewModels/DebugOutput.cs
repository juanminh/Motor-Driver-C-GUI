using MotorController.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorController.ViewModels
{
    public class DebugOutput : ViewModelBase
    {
        private static DebugOutput _instance;
        private static readonly object Synlock = new object(); //Single tone variabl
        public static DebugOutput GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new DebugOutput();
                    return _instance;
                }
            }
        }
        public DebugOutput() { }

        private int _id = 5;
        private int _subid = 1;
        public int ID { get { return _id; } set { _id = value; } }
        public int subID { get { return _subid; } set { _subid = value; } }
        public string IDstr { get { return ID.ToString(); }}
        public string subIDstr { get { return subID.ToString(); } }

    }
}
