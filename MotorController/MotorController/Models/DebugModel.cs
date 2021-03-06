﻿using System;
using System.Windows.Input;
using MotorController.Models;
using MotorController.Models.DriverBlock;
using MathNet.Numerics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Linq;
using MotorController.Common;
using Abt.Controls.SciChart;
using MotorController.Views;
using System.ComponentModel;

namespace MotorController.Models
{
    class DebugModel
    {
        private string _id;
        private string _index;
        private string _getData;
        private string _setData;
        private bool _intfloat = true; // true = int, false = float
        public bool IntFloat
        {
            get { return _intfloat; }
            set { _intfloat = value; }
        }
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Index
        {
            get { return _index; }
            set { _index = value; }
        }
        public string GetData
        {
            get { return _getData; }
            set { _getData = value; }
        }
        public string SetData
        {
            get { return _setData; }
            set { _setData = value; }
        }
    }
}
