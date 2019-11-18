using Abt.Controls.SciChart;
using SuperButton.CommandsDB;
using SuperButton.Models;
using SuperButton.Models.DriverBlock;
using SuperButton.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections;
using System.Runtime.CompilerServices;

namespace SuperButton.ViewModels
{
    public class DebugObjModel : ViewModelBase
    {
        private DebugModel _debugModel = new DebugModel();

        public bool IntFloat
        {
            get { return _debugModel.IntFloat; }
            set
            {
                _debugModel.IntFloat = value;
                OnPropertyChanged("IntFloat");
            }
        }
        public string ID
        {
            get { return _debugModel.ID; }
            set {
                _debugModel.ID = value;
                OnPropertyChanged("ID");
            }
        }
        public string Index
        {
            get { return _debugModel.Index; }
            set {
                _debugModel.Index = value; OnPropertyChanged("Index"); 
            }
        }
        public string GetData
        {
            get
            {
                return _debugModel.GetData;
            }
            set
            {
                _debugModel.GetData = value;
                OnPropertyChanged();
            }
        }
        public string SetData
        {
            get { return _debugModel.SetData; }
            set {
                _debugModel.SetData = value;
                OnPropertyChanged();
            }
        }

        public ActionCommand Get { get { return new ActionCommand(GetCmd); } }
        public ActionCommand Set { get { return new ActionCommand(SetCmd); } }

        private void GetCmd()
        {
            DebugViewModel.GetInstance.TxBuildOperation("", Convert.ToInt16(ID), Convert.ToInt16(Index), false, !this.IntFloat);
            try
            {
                var tmp = new PacketFields
                {
                    Data2Send = 0,
                    ID = Convert.ToInt16(ID),
                    SubID = Convert.ToInt16(Index),
                    IsSet = false,
                    IsFloat = !this.IntFloat,
                };
                Task.Factory.StartNew(action: () => { Rs232Interface.GetInstance.SendToParser(tmp); });
            }
            catch(Exception)
            {
            }
        }
        private void SetCmd()
        {
            if(SetData != "" && ID != "" && Index != "")
            {
                DebugViewModel.GetInstance.TxBuildOperation(SetData, Convert.ToInt16(ID), Convert.ToInt16(Index), true, !this.IntFloat);
                try
                {
                    var tmp = new PacketFields
                    {
                        Data2Send = SetData,
                        ID = Convert.ToInt16(ID),
                        SubID = Convert.ToInt16(Index),
                        IsSet = true,
                        IsFloat = !this.IntFloat,
                    };
                    Task.Factory.StartNew(action: () => { Rs232Interface.GetInstance.SendToParser(tmp); });
                }
                catch { }
            }
        }
    }
}
