using System;
using System.Collections.ObjectModel;
using MotorController.Common;
using MotorController.Models.DriverBlock;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using MotorController.Helpers;
using System.Linq;
using MotorController.Views;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Abt.Controls.SciChart;
using MotorController.ViewModels;
using System.Runtime.InteropServices;

namespace MotorController.ViewModels
{
    class MaintenanceViewModel : ViewModelBase
    {
        string iniPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MotorController\\SerialProgrammer\\SerialProgrammer.ini"; // Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        iniFile _serial_programmer_parameters;

        private static readonly object Synlock = new object();
        private static MaintenanceViewModel _instance;
        public static MaintenanceViewModel GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new MaintenanceViewModel();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        private MaintenanceViewModel()
        {
            foreach(var fb in EnumHelper.GetNames(Enum.GetNames(typeof(eBaudRate))))
                _flashBaudrateList.Add(fb.TrimStart(new char[] { 'B', 'a', 'u', 'd' }));

            _serial_programmer_parameters = new iniFile(iniPath);
            // section, key, value, _iniFile
            pathFWtemp = _serial_programmer_parameters.Read("Firmware Path", "Programmer");
            PathFW = ShortenPath(pathFWtemp);
            FlashBaudRate = _serial_programmer_parameters.Read("FlashBaud", "Programmer");
            if(String.IsNullOrWhiteSpace(FlashBaudRate))
                FlashBaudRate = "230400";

        }

        #region DriverParameters

        public static List<UInt32> ParamsToFile = new List<UInt32>();
        public static List<UInt32> FileToParams = new List<UInt32>();
        private bool _saveToFile = false;
        public static UInt32 ParamsCount = 0;
        public static UInt32 PbarParamsCount = 0;
        private string _pathToFile, _pathFromFile;
        public string PathToFile
        {
            get { return _pathToFile; }
            set { _pathToFile = value; OnPropertyChanged("PathToFile"); }
        }
        public string PathFromFile
        {
            get { return _pathFromFile; }
            set { _pathFromFile = value; OnPropertyChanged("PathFromFile"); }
        }
        public ActionCommand OpenToFileCmd
        {
            get { return new ActionCommand(OpenToFile); }
        }
        public ActionCommand OpenFromFileCmd
        {
            get { return new ActionCommand(OpenFromFile); }
        }
        public void OpenToFile()
        {
            string tempPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MotorController\\Parameters\\";
            if(!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            Process.Start(tempPath);
        }
        public static string pathFromFiletemp = "";
        public void OpenFromFile()
        {
            System.Windows.Forms.OpenFileDialog ChooseFile = new System.Windows.Forms.OpenFileDialog();
            ChooseFile.Filter = "All Files (*.*)|*.*";
            ChooseFile.FilterIndex = 1;

            ChooseFile.Multiselect = false;
            string[] words = pathFromFiletemp.Split('\\');

            pathFromFiletemp = "";
            for(int i = 0; i < words.Length - 1; i++)
                pathFromFiletemp += words[i] + '\\';
            if(String.IsNullOrEmpty(pathFromFiletemp))
                ChooseFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MotorController\\Parameters\\";
            else
                ChooseFile.InitialDirectory = pathFromFiletemp;

            if(!Directory.Exists(ChooseFile.InitialDirectory))
                Directory.CreateDirectory(ChooseFile.InitialDirectory);

            if(ChooseFile.ShowDialog() == DialogResult.OK)
            {
                pathFromFiletemp = ChooseFile.FileName;
                PathFromFile = ShortenPath(ChooseFile.FileName);
            }
        }
        public bool SaveToFile
        {
            get { return _saveToFile; }
            set
            {
                if(value && LeftPanelViewModel._app_running)
                {
                    PbarValueToFile = 0;
                    _redoState = PreRedoState(OscilloscopeParameters.ChanTotalCounter, DebugViewModel.GetInstance.EnRefresh);
                    {
                        _saveToFile = value;
                        ParamsToFile.Clear();
                        
                        Rs232Interface.GetInstance.SendToParser(new PacketFields
                        {
                            Data2Send = 0,
                            ID = 67,
                            SubID = Convert.ToInt16(1),
                            IsSet = false,
                            IsFloat = false
                        });
                        //var task1 = Task.Factory.StartNew(action: () =>
                        //{
                            CheckPBar("ToFile");
                        //});
                        //Task.WaitAll(task1);
                    }
                }
                else if(!value)
                {
                    if(PbarValueToFile == 0 || PbarValueToFile == 100)
                        _saveToFile = value;
                }

                OnPropertyChanged();
            }
        }
        private bool _loadFromFile = false;
        public bool LoadFromFile
        {
            get { return _loadFromFile; }
            set
            {
                if(value && LeftPanelViewModel._app_running)
                {
                    _redoState = PreRedoState(OscilloscopeParameters.ChanTotalCounter, DebugViewModel.GetInstance.EnRefresh);
                    PbarValueFromFile = 0;
                    if(String.IsNullOrWhiteSpace(PathFromFile))
                    {
                        EventRiser.Instance.RiseEevent(string.Format($"Please choose a file and retry!"));
                        _loadFromFile = !value;
                        OnPropertyChanged("LoadFromFile");
                    }
                    else
                    {
                        FileToParams.Clear();
                        _loadFromFile = value;
                        OnPropertyChanged("LoadFromFile");
                        Rs232Interface.GetInstance.SendToParser(new PacketFields
                        {
                            Data2Send = 0,
                            ID = 67,
                            SubID = Convert.ToInt16(1),
                            IsSet = false,
                            IsFloat = false
                        });
                        CheckPBar("FromFile");
                    }
                }
                else if(!value)
                {
                    if(PbarValueFromFile == 0 || PbarValueFromFile == 100)
                    {
                        _loadFromFile = value;
                        OnPropertyChanged("LoadFromFile");
                    }
                }
            }
        }
        public void DataToList(UInt32 data)
        {
            if(ParamsCount > 0)
            {
                ParamsCount -= 1;
                ParamsToFile.Add(data);
                PbarValueToFile = (ParamsToFile.Count) * 100 / PbarParamsCount;
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = 1,
                    ID = 67,
                    SubID = Convert.ToInt16(13),
                    IsSet = false,
                    IsFloat = false
                });
            }
            else
            {
                ParamsToFile.Add(data);
                UInt32 Sum = 0;
                for(int i = 0; i < ParamsToFile.Count - 1; i++)
                {
                    Sum += ((ParamsToFile.ElementAt(i) >> 16) & 0xFFFF) + (ParamsToFile.ElementAt(i) & 0xFFFF);
                }
                if(ParamsToFile.ElementAt(ParamsToFile.Count - 1) == Sum)
                {
                    SaveToFile = false;
                    SaveToFileFunc(ParamsToFile);
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = 0,
                        ID = 67,
                        SubID = Convert.ToInt16(12),
                        IsSet = true,
                        IsFloat = false
                    });
                    EventRiser.Instance.RiseEevent(string.Format($"Load Parameters succeed"));
                    PostRedoState(_redoState);

                }
                else
                {
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = 0,
                        ID = 67,
                        SubID = Convert.ToInt16(12),
                        IsSet = true,
                        IsFloat = false
                    });
                    EventRiser.Instance.RiseEevent(string.Format($"Checksum Failed!"));
                    SaveToFileFunc(ParamsToFile);
                }
            }
        }
        public static string pathToFiletemp = "";
        public void SaveToFileFunc(List<UInt32> ListToSave)
        {
            //System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            //form.TopMost = true;

            string Date = Day(DateTime.Now.Day) + ' ' + MonthTrans(DateTime.Now.Month) + ' ' + DateTime.Now.Year.ToString();
            string path = "\\MotorController\\Parameters\\" + Date + ' ' + DateTime.Now.ToString("HH:mm:ss");
            path = (path.Replace('-', ' ')).Replace(':', '_');
            path += ".txt";
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + path;

            System.Windows.Forms.SaveFileDialog saveFile = new System.Windows.Forms.SaveFileDialog();
            saveFile.Filter = "Text (*.txt)|*.txt";
            saveFile.FileName = path;

            var t = new Thread((ThreadStart)(() =>
            {
                if(saveFile.ShowDialog() == DialogResult.OK)
                {
                    pathToFiletemp = saveFile.FileName;
                    PathToFile = ShortenPath(pathToFiletemp);
                }
                else
                    return;
                using(StreamWriter writer = new StreamWriter(File.Open(pathToFiletemp, FileMode.Create)))
                {
                    foreach(var item in ListToSave)
                    {
                        writer.Write(item.ToString("X2").PadLeft(8, '0'));
                        writer.Write(" ");
                    }
                }
            }));
            PathToFile = ShortenPath(pathToFiletemp);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            //t.Join();
        }
        public static string MonthTrans(int month)
        {
            switch(month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "Septembre";
                case 10:
                    return "Octobre";
                case 11:
                    return "Novembre";
                case 12:
                    return "Decembre";
                default:
                    return "x";
            }

        }
        public static string Day(int day)
        {
            if(day < 10)
            {
                return "0" + day.ToString();
            }
            else
                return day.ToString();

        }
        public bool SelectFile(UInt32 ParamsCount)
        {
            string readText = File.ReadAllText(pathFromFiletemp);
            var array = readText.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            foreach(var elements in array)
                FileToParams.Add(Convert.ToUInt32(elements, 16));
            if(ParamsCount == FileToParams.Count() - 1)
            {
                return true;
            }
            else
            {
                EventRiser.Instance.RiseEevent(string.Format($"Wrong File Detected!"));
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = 0,
                    ID = 67,
                    SubID = Convert.ToInt16(2),
                    IsSet = true,
                    IsFloat = false
                });
                return false;
            }
            //LoadFromFile = false;
        }
        private long _pbarValueFromFile = 0;
        public long PbarValueFromFile
        {
            get { return _pbarValueFromFile; }
            set { _pbarValueFromFile = value; OnPropertyChanged("PbarValueFromFile"); }
        }
        private long _pbarValueToFile = 0;
        public long PbarValueToFile
        {
            get { return _pbarValueToFile; }
            set { _pbarValueToFile = value; OnPropertyChanged("PbarValueToFile"); }
        }
        private void CheckPBar(string way)
        {
            int timeout = 0;
            long tempPbarVal = 0;
            if(way == "ToFile")
            {
                Task.Factory.StartNew(action: () =>
                {
                    Thread.Sleep(1);
                    while(PbarValueToFile != 100 && timeout < 100)
                    {
                        if(tempPbarVal != PbarValueToFile)
                        {
                            tempPbarVal = PbarValueToFile;
                            timeout = 0;
                        }
                        else
                        {
                            timeout++;
                            Thread.Sleep(100);
                        }
                        if(PbarValueToFile == 100)
                            break;
                        if(timeout >= 100)
                        {
                            SaveToFile = false;
                            PbarValueToFile = 0;
                            EventRiser.Instance.RiseEevent(string.Format($"Load Parameters Failed"));
                            SaveToFile = false;
                            LoadFromFile = false;
                            PostRedoState(_redoState);
                        }
                    }
                });
            }
            else
            {
                Task.Factory.StartNew(action: () =>
                {
                    Thread.Sleep(1);
                    while(PbarValueFromFile != 100 && timeout < 100)
                    {
                        if(tempPbarVal != PbarValueFromFile)
                        {
                            tempPbarVal = PbarValueFromFile;
                            timeout = 0;
                        }
                        else
                            timeout++;
                        Thread.Sleep(100);
                        if(PbarValueFromFile == 100)
                            break;
                        if(timeout >= 100)
                        {
                            SaveToFile = false;
                            PbarValueFromFile = 0;
                            EventRiser.Instance.RiseEevent(string.Format($"Load Parameters Failed"));
                            SaveToFile = false;
                            LoadFromFile = false;
                            PostRedoState(_redoState);
                        }
                    }
                });
            }
        }

        public void data_transfert(int commandId, int commandSubId, int getSet, Int32 transit)
        {
            if(commandSubId == 1)
            {
                PbarParamsCount = Convert.ToUInt32(transit);
                if(SaveToFile == true)
                {
                    ParamsCount = Convert.ToUInt32(transit);
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = 1,
                        ID = 67,
                        SubID = Convert.ToInt16(12),
                        IsSet = true,
                        IsFloat = false
                    }
                    );
                }
                else if(LoadFromFile == true)
                {
                    if(SelectFile(Convert.ToUInt32(transit)))
                    {
                        Rs232Interface.GetInstance.SendToParser(new PacketFields
                        {
                            Data2Send = 1,
                            ID = 67,
                            SubID = Convert.ToInt16(2),
                            IsSet = true,
                            IsFloat = false
                        });
                    }
                    else
                        LoadFromFile = false;
                }
            }
            else if(commandSubId == 12 && getSet == 0)
            {
                if(ParamsToFile.Count == 0)
                {
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = 1,
                        ID = 67,
                        SubID = Convert.ToInt16(13),
                        IsSet = false,
                        IsFloat = false
                    }
                    );
                }
                else
                    SaveToFile = false;
            }
            else if(commandSubId == 13)
                DataToList(Convert.ToUInt32((uint)transit));
            else if(commandSubId == 2 && getSet == 0)
            {
                if(LoadFromFile && FileToParams.Count > 1)
                {
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = FileToParams.ElementAt(0),
                        ID = 67,
                        SubID = Convert.ToInt16(3),
                        IsSet = true,
                        IsFloat = false
                    });
                    FileToParams.RemoveAt(0);
                }
                else if(!LoadFromFile && FileToParams.Count > 1)
                    LoadFromFile = false;
            }
            else if(commandSubId == 3 && getSet == 0)
            {
                if(FileToParams.Count > 1)
                {
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = (int)(FileToParams.ElementAt(0)),
                        ID = 67,
                        SubID = Convert.ToInt16(3),
                        IsSet = true,
                        IsFloat = false
                    });
                    FileToParams.RemoveAt(0);
                    PbarValueFromFile = 100 - ((FileToParams.Count) * 100 / PbarParamsCount);
                }
                else if(FileToParams.Count == 1)
                {
                    Rs232Interface.GetInstance.SendToParser(new PacketFields
                    {
                        Data2Send = (int)(FileToParams.ElementAt(0)),
                        ID = 67,
                        SubID = Convert.ToInt16(4),
                        IsSet = true,
                        IsFloat = false
                    });
                }
            }
            else if(commandSubId == 4 && getSet == 0)
            {
                if(transit == 1)
                {
                    EventRiser.Instance.RiseEevent(string.Format($"Load Parameters succeed"));
                    PostRedoState(_redoState);
                }
                else
                {
                    EventRiser.Instance.RiseEevent(string.Format($"Load Parameters Failed"));
                    SaveToFile = false;
                    LoadFromFile = false;
                    PostRedoState(_redoState);
                }
                LoadFromFile = false;

                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = 0,
                    ID = 67,
                    SubID = Convert.ToInt16(2),
                    IsSet = true,
                    IsFloat = false
                });
            }
        }
        #endregion DriverParameters

        #region SerialProgrammer
        private string _pathFW = "";
        public string PathFW
        {
            get { return _pathFW; }
            set { _pathFW = value; OnPropertyChanged("PathFW"); }
        }
        public ActionCommand OpenPathFWCmd
        {
            get { return new ActionCommand(OpenPathFW); }
        }
        public static string pathFWtemp = "";
        public void OpenPathFW()
        {
            System.Windows.Forms.OpenFileDialog ChooseFile = new System.Windows.Forms.OpenFileDialog();
            ChooseFile.Filter = "All Files (*.*)|*.*";
            ChooseFile.FilterIndex = 1;

            ChooseFile.Multiselect = false;
            string[] words = pathFWtemp.Split('\\');

            pathFWtemp = "";
            for(int i = 0; i < words.Length - 1; i++)
                pathFWtemp += words[i] + '\\';
            if(String.IsNullOrEmpty(pathFWtemp))
                ChooseFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MotorController\\FirmwareUpdate\\";
            else
                ChooseFile.InitialDirectory = pathFWtemp;

            if(!Directory.Exists(ChooseFile.InitialDirectory))
                Directory.CreateDirectory(ChooseFile.InitialDirectory);
            if(ChooseFile.ShowDialog() == DialogResult.OK)
            {
                pathFWtemp = ChooseFile.FileName;
                PathFW = ShortenPath(ChooseFile.FileName);
            }
        }
        private long _pbarValueFW = 0;
        public long PbarValueFW
        {
            get { return _pbarValueFW; }
            set { _pbarValueFW = value; OnPropertyChanged("PbarValueFW"); }
        }
        public CancellationToken cancellationTokenSerialProgrammer;
        private bool _serialProgrammer;
        public bool SerialProgrammerCheck
        {
            get { return _serialProgrammer; }
            set
            {
                _serialProgrammer = value;
                OnPropertyChanged("SerialProgrammerCheck");
                if(value)
                {
                    SerialProgrammerFunc();
                }
                else if(_serialProgrammerStarted)
                    cancellationTokenSerialProgrammer = new CancellationToken(true);
            }
        }
        public ActionCommand OpenSerialProgrammer
        {
            get { return new ActionCommand(SerialProgrammerFunc); }
        }
        public bool _serialProgrammerStarted = false;
        private async void SerialProgrammerFunc()
        {
            _serialProgrammerStarted = true;
            cancellationTokenSerialProgrammer = new CancellationToken(false);
            try
            {
                await Task.Run(() => SerialProgrammer.GetInstance.SerialProgrammerProcess());
            }
            catch
            { }
            SerialProgrammerCheck = false;
            _serialProgrammerStarted = false;

            _serial_programmer_parameters.Write("Firmware Path", pathFWtemp, "Programmer");
            _serial_programmer_parameters.Write("FlashBaud", FlashBaudRate, "Programmer");
        }
        //public void SerialProgrammerApp()
        //{
        //    Process[] Proc = Process.GetProcessesByName("Serial Programmer");
        //    while(Proc.Length != 0)
        //    {
        //        Proc = Process.GetProcessesByName("Serial Programmer");
        //        Thread.Sleep(500);
        //        Debug.WriteLine("app");
        //    }
        //    Debug.WriteLine("app closed");
        //    LeftPanelViewModel.GetInstance.AutoConnectCommand();
        //}
        private ObservableCollection<string> _flashBaudrateList = new ObservableCollection<string>();
        public ObservableCollection<string> FlashBaudrateList
        {

            get
            {
                return _flashBaudrateList;
            }
            set
            {
                _flashBaudrateList = value;
                OnPropertyChanged();
            }

        }
        private string _flashBaudRate = "230400";
        public string FlashBaudRate
        {
            get
            {
                return _flashBaudRate;
            }
            set
            {
                _flashBaudRate = value;
                OnPropertyChanged();
            }
        }
        #endregion SerialProgrammer        

        public static bool CurrentButton = false;
        public static bool DefaultButton = false;

        public static string ShortenPath(string path, int maxLength = 35)
        {
            string ellipsisChars = "...";
            char dirSeperatorChar = Path.DirectorySeparatorChar;
            string directorySeperator = dirSeperatorChar.ToString();

            //simple guards
            if(path.Length <= maxLength)
            {
                return path;
            }
            int ellipsisLength = ellipsisChars.Length;
            if(maxLength <= ellipsisLength)
            {
                return ellipsisChars;
            }


            //alternate between taking a section from the start (firstPart) or the path and the end (lastPart)
            bool isFirstPartsTurn = true; //drive letter has first priority, so start with that and see what else there is room for

            //vars for accumulating the first and last parts of the final shortened path
            string firstPart = "";
            string lastPart = "";
            //keeping track of how many first/last parts have already been added to the shortened path
            int firstPartsUsed = 0;
            int lastPartsUsed = 0;

            string[] pathParts = path.Split(dirSeperatorChar);
            for(int i = 0; i < pathParts.Length; i++)
            {
                if(isFirstPartsTurn)
                {
                    string partToAdd = pathParts[firstPartsUsed] + directorySeperator;
                    if((firstPart.Length + lastPart.Length + partToAdd.Length + ellipsisLength) > maxLength)
                    {
                        break;
                    }
                    firstPart = firstPart + partToAdd;
                    if(partToAdd == directorySeperator)
                    {
                        //this is most likely the first part of and UNC or relative path 
                        //do not switch to lastpart, as these are not "true" directory seperators
                        //otherwise "\\myserver\theshare\outproject\www_project\file.txt" becomes "\\...\www_project\file.txt" instead of the intended "\\myserver\...\file.txt")
                    }
                    else
                    {
                        isFirstPartsTurn = false;
                    }
                    firstPartsUsed++;
                }
                else
                {
                    int index = pathParts.Length - lastPartsUsed - 1; //-1 because of length vs. zero-based indexing
                    string partToAdd = directorySeperator + pathParts[index];
                    if((firstPart.Length + lastPart.Length + partToAdd.Length + ellipsisLength) > maxLength)
                    {
                        break;
                    }
                    lastPart = partToAdd + lastPart;
                    if(partToAdd == directorySeperator)
                    {
                        //this is most likely the last part of a relative path (e.g. "\websites\myproject\www_myproj\App_Data\")
                        //do not proceed to processing firstPart yet
                    }
                    else
                    {
                        isFirstPartsTurn = true;
                    }
                    lastPartsUsed++;
                }
            }

            if(lastPart == "")
            {
                //the filename (and root path) in itself was longer than maxLength, shorten it
                lastPart = pathParts[pathParts.Length - 1];//"pathParts[pathParts.Length -1]" is the equivalent of "Path.GetFileName(pathToShorten)"
                lastPart = lastPart.Substring(lastPart.Length + ellipsisLength + firstPart.Length - maxLength, maxLength - ellipsisLength - firstPart.Length);
            }

            return firstPart + ellipsisChars + lastPart;
        }
        public string PreRedoState(int plot, bool refresh)
        {
            LeftPanelViewModel.GetInstance.VerifyConnectionTicks(LeftPanelViewModel.STOP);
            if(plot > 0 && refresh)
            {
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = 0,
                    ID = 64,
                    SubID = Convert.ToInt16(0),
                    IsSet = true,
                    IsFloat = false
                });
                Thread.Sleep(10);
                DebugViewModel.GetInstance.EnRefresh = false;
                Thread.Sleep(10);
                return "both";
            }
            else if(plot > 0)
            {
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = 0,
                    ID = 64,
                    SubID = Convert.ToInt16(0),
                    IsSet = true,
                    IsFloat = false
                });
                return "plot";
            }
            else if(refresh)
            {
                Thread.Sleep(10);
                DebugViewModel.GetInstance.EnRefresh = false;
                Thread.Sleep(10);
                return "refresh";
            }
            else
                return "nothing";

        }
        public void PostRedoState(string state)
        {
            LeftPanelViewModel.GetInstance.VerifyConnectionTicks(LeftPanelViewModel.START);
            if(state == "both")
            {
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = 1,
                    ID = 64,
                    SubID = Convert.ToInt16(0),
                    IsSet = true,
                    IsFloat = false
                });
                Thread.Sleep(10);
                DebugViewModel.GetInstance.EnRefresh = true;
                Thread.Sleep(10);
            }
            else if(state == "refresh")
            {
                Thread.Sleep(10);
                DebugViewModel.GetInstance.EnRefresh = true;
                Thread.Sleep(10);
            }
            else if(state == "plot")
            {
                Rs232Interface.GetInstance.SendToParser(new PacketFields
                {
                    Data2Send = 1,
                    ID = 64,
                    SubID = Convert.ToInt16(0),
                    IsSet = true,
                    IsFloat = false
                });
            }
        }
        public static string _redoState = "";

        #region NewToggleSwitch
        public ObservableCollection<object> MaintenanceOperation
        {

            get
            {
                return Commands.GetInstance.GenericCommandsGroup["MaintenanceOperation"];
            }
        }
        #endregion NewToggleSwitch
    }
}
