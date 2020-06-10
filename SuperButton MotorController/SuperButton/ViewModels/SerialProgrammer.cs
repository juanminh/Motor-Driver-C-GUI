//#define SerialProgrammerLabVIEW
#define Learning
using SuperButton.Models.DriverBlock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using SuperButton.Models.ParserBlock;
using System.Diagnostics;
using Timer = System.Timers.Timer;
using System.Windows.Forms;
using System.IO;
using SuperButton.Helpers;
using SuperButton.Common;

namespace SuperButton.ViewModels
{
    enum eInput
    {
        PASS,
        FAIL
    }
    enum eSTAGE
    {
        IDLE = 0,
        INIT_VARIABLES = 1,
        CONNECT_COM = 2,
        AUTOBAUD_DRIVER = 3,
        BOOT_COMMAND = 4,
        FLASH_BAUD = 5,
        AUTOBAUD = 6,
        ERASE = 7,
        PROGRAM = 8,
        RESET = 9,
        FW_VERSION = 10,
        RECONNECT = 11,
        RESET_DRIVER = 12,
        EXIT = 13
    }
    enum eBaudRate
    {
        Baud110 = 110,
        Baud300 = 300,
        Baud600 = 600,
        Baud1200 = 1200,
        Baud2400 = 2400,
        Baud4800 = 4800,
        Baud9600 = 9600,
        Baud19200 = 19200,
        Baud38400 = 38400,
        Baud57600 = 57600,
        Baud115200 = 115200,
        Baud230400 = 230400,
        Baud460800 = 460800,
        Baud912600 = 912600
    }
    public class SerialProgrammer
    {
        int MAX_TIMEOUT = 50;
        int MAX_TIMEOUT_CS = 500;

        public static CrcEventhandlerCalcHostFrameCrc CrcInputCalc = CrcBase.CalcHostFrameCrc;
        public byte[] temp;
        public int _currentState = (int)eSTAGE.IDLE;
        private string _filePath = "";
        private static List<Byte> _dataFromFile = new List<Byte>();


        private static readonly object Synlock = new object();
        private static SerialProgrammer _instance;
        public static SerialProgrammer GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new SerialProgrammer();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        private SerialProgrammer()
        {
        }

        #region INIT_VARIABLES
        bool wait = true;
        byte[] expectedResponse = { 0 };             //  Requiered response => 139 60 134 0 0 79 218 62 0 192 135
        int timeout = 0, foundBaudrate = 0, equal = 0;
        string readText;
        Int64 dataFileCount;
        byte[] _header = new byte[22];
        UInt16 _checkSum = 0;
        #endregion INIT_VARIABLES

        public void SerialProgrammerProcess()
        {
#if SerialProgrammerLabVIEW
            string iniPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // section, key, value, _iniFile
            iniFile.WritePrivateProfileString("Programmer", "CmdBaud", Rs232Interface.GetInstance.BaudRate, iniPath + "\\MotorController\\SerialProgrammer\\SerialProgrammer.ini");
            iniFile.WritePrivateProfileString("Programmer", "COM", LeftPanelViewModel.GetInstance.ComboBoxCOM.ComString, iniPath + "\\MotorController\\SerialProgrammer\\SerialProgrammer.ini");

            if(LeftPanelViewModel.GetInstance.ConnectTextBoxContent != "Not Connected")
                Rs232Interface.GetInstance.Disconnect();

            int countDisconnection = 0;
            while(LeftPanelViewModel.GetInstance.ConnectTextBoxContent != "Not Connected")
            {
                Thread.Sleep(500);
                Debug.WriteLine("wait end connection");
                countDisconnection++;
                if(countDisconnection == 3)
                    Rs232Interface.GetInstance.Disconnect();
            }

            Process.Start(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\SerialProgrammer\\Serial Programmer.exe");

            Thread SerialProgrammer = new Thread(MaintenanceViewModel.GetInstance.SerialProgrammerApp);
            SerialProgrammer.Start();
#else
            _serialProgrammer();
#endif
        }
        public void _serialProgrammer()
        {
            _currentState = (int)eSTAGE.INIT_VARIABLES;

            while(true)
            {
                Debug.WriteLine(_currentState.ToString());
                if(MaintenanceViewModel.GetInstance.cancellationTokenSerialProgrammer.IsCancellationRequested)
                {
                    EventRiser.Instance.RiseEevent("Operation cancelled by user");
                    _currentState = (int)eSTAGE.EXIT;
                }
                switch(_currentState)
                {
                    case (int)eSTAGE.IDLE:
                        break;
                    case (int)eSTAGE.INIT_VARIABLES:
                        if(ini_variables())
                        {
                            if(LeftPanelViewModel._app_running)
                                _currentState = (int)eSTAGE.BOOT_COMMAND;
                            else
                                _currentState = (int)eSTAGE.CONNECT_COM;
                        }
                        else
                            _currentState = (int)eSTAGE.EXIT;
                        break;
                    case (int)eSTAGE.CONNECT_COM:
                        _currentState = (int)eSTAGE.AUTOBAUD_DRIVER;
                        break;
                    case (int)eSTAGE.AUTOBAUD_DRIVER:
                        if(connect_com())
                            _currentState = (int)eSTAGE.BOOT_COMMAND;
                        else
                            _currentState = (int)eSTAGE.FLASH_BAUD;
                        break;
                    case (int)eSTAGE.BOOT_COMMAND:
                        if(LeftPanelViewModel._app_running)
                        {
                            if(Rs232Interface._comPort != null)
                                Rs232Interface._comPort.DataReceived -= Rs232Interface.GetInstance.DataReceived;
                            if(Rs232Interface._comPort != null)
                                Rs232Interface._comPort.Close();
                            if(Rs232Interface._comPort != null)
                                Rs232Interface._comPort.Dispose();
                            PortChat.GetInstance.CloseComunication();
                            PortChat.GetInstance.Main(Configuration.SelectedCom, foundBaudrate);
                            PortChat.GetInstance.ReadTick((int)(eSTATE.START));
                            PortChat.GetInstance._packetsList.Clear();
                        }
                        if(boot_command())
                            _currentState = (int)eSTAGE.FLASH_BAUD;
                        else
                            _currentState = (int)eSTAGE.FLASH_BAUD;
                        break;
                    case (int)eSTAGE.FLASH_BAUD:
                        if(flash_baud())
                            _currentState = (int)eSTAGE.AUTOBAUD;
                        else
                            _currentState = (int)eSTAGE.EXIT;
                        break;
                    case (int)eSTAGE.AUTOBAUD:
                        if(autobaud())
                            _currentState = (int)eSTAGE.ERASE;
                        else
                            _currentState = (int)eSTAGE.EXIT;
                        break;
                    case (int)eSTAGE.ERASE:
                        if(erase())
                            _currentState = (int)eSTAGE.PROGRAM;
                        else
                            _currentState = (int)eSTAGE.EXIT;
                        break;
                    case (int)eSTAGE.PROGRAM:
                        if(program())
                            _currentState = (int)eSTAGE.RESET;
                        else
                            _currentState = (int)eSTAGE.EXIT;
                        break;
                    case (int)eSTAGE.RESET:
                        reset();
                        _currentState = (int)eSTAGE.FW_VERSION;
                        break;
                    case (int)eSTAGE.FW_VERSION:
                        fw_version();
                        _currentState = (int)eSTAGE.RESET_DRIVER;
                        break;
                    case (int)eSTAGE.RESET_DRIVER:
                        reset_driver();
                        _currentState = (int)eSTAGE.RECONNECT;
                        break;
                    case (int)eSTAGE.RECONNECT:
                        reconnect();
                        _currentState = (int)eSTAGE.EXIT;
                        break;
                    case (int)eSTAGE.EXIT:
                    default:
                        exit();
                        return;
                }
            }
        }
        private bool sendCS(byte[] Buf, UInt16 expectedCS, int size = 2)
        {
            bool wait = true;
            int equal = 0;
            int timeout = 0;
            byte[] expectedResponse = new byte[] { (byte)(expectedCS & 0xFF), (byte)(expectedCS >> 8) };

            PortChat.GetInstance._packetsList.Clear();
            PortChat.Send(Buf);
            try
            {
                //Debug.WriteLine(DateTime.Now.ToString() + "." + DateTime.Now.Millisecond.ToString());
                while(wait && timeout < MAX_TIMEOUT_CS)
                {
                    if(PortChat.GetInstance._packetsList.Count != 0)
                    {
                        if(expectedCS == 0)
                        {
                            expectedResponse = new byte[] {  100, 0, 0, 200, 0, 0, 0, 27, 166 };
                            size = 9;
                        }
                        for(int i = 0; i < PortChat.GetInstance._packetsList.ElementAt(0).Count(); i++)
                        {
                                if(PortChat.GetInstance._packetsList.ElementAt(0)[i] == expectedResponse[i])
                                    equal++;
                        }
                        if(equal == size)
                        {
                            wait = false;
                            //for(int k = 0; k < size; k++)
                            //    Debug.WriteLine(" rcv " + (int)(PortChat.GetInstance._packetsList.ElementAt(k)[0]));
                        }
                    }
                    timeout++;
                    Thread.Sleep(1);
                }
                //Debug.WriteLine(DateTime.Now.ToString() + "." + DateTime.Now.Millisecond.ToString());
            }
            catch
            {
                Debug.WriteLine("Error: " + DateTime.Now.ToString() + "." + DateTime.Now.Millisecond.ToString());
            }
            if(timeout == MAX_TIMEOUT_CS)
                return false;
            else
                return true;
        }
        public void ParseOutputData(object Data2Send, Int16 Id, Int16 SubId, bool IsSet, bool IsFloat)
        {
            char tempChar = (char)0;

            if(IsSet)
                temp = new byte[11] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            else
                temp = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };

            temp[0] = 0x49;             //PreambleLSByte
            temp[1] = 0x5d;             //PreambleMsbyte
            temp[2] = (byte)(Id);       // ID msb
            tempChar = (char)(tempChar | ((char)(((Id >> 8)) & 0x3F)) | (((char)(SubId & 0x3)) << 6));
            temp[3] = (byte)tempChar;
            tempChar = (char)0;
            tempChar = (char)(tempChar | (char)(SubId >> 2));
            temp[4] = (byte)tempChar;

            if(IsSet == false) // Set/Get
                temp[4] |= (1 << 4);
            if(IsFloat)        // Float/Int
                temp[4] |= (1 << 5);
            if(IsSet == false)
            {
                //Risng up delegate , to call static function from CRC class
                ushort TempGetCrc = CrcInputCalc(temp.Take(5), 2);
                temp[5] = (byte)(TempGetCrc & 0xFF);
                temp[6] = (byte)((TempGetCrc >> 8) & 0xFF);

                PortChat.Send(temp);
                return;
            }

            if(Data2Send is double)                                           //Data float
            {
                var datvaluevalue = BitConverter.GetBytes((float)(Data2Send is double ? (double)Data2Send : 0));
                temp[5] = (byte)(datvaluevalue[0]);
                temp[6] = (byte)(datvaluevalue[1]);
                temp[7] = (byte)(datvaluevalue[2]);
                temp[8] = (byte)(datvaluevalue[3]);
            }
            else if(Data2Send is int)   //Data int
            {
                temp[5] = (byte)(((int)Data2Send & 0xFF));
                temp[6] = (byte)(((int)Data2Send >> 8) & 0xFF);
                temp[7] = (byte)(((int)Data2Send >> 16) & 0xFF);
                temp[8] = (byte)(((int)Data2Send >> 24) & 0xFF);
            }
            else if(IsFloat)  //Data float
            {
                float value = 0;
                float.TryParse((string)Data2Send, out value);
                byte[] _value = BitConverter.GetBytes(value);
                temp[5] = (byte)(_value[0]);
                temp[6] = (byte)(_value[1]);
                temp[7] = (byte)(_value[2]);
                temp[8] = (byte)(_value[3]);
            }
            else // String Value
            {
                if(Data2Send.ToString().Length != 0)
                {
                    if(Data2Send.ToString().IndexOf(".") != -1)
                        Data2Send = Data2Send.ToString().Substring(0, Data2Send.ToString().IndexOf("."));
                    try
                    {
                        var datvaluevalue = 0;
                        Int32.TryParse(Data2Send.ToString(), out datvaluevalue);
                        temp[5] = (byte)(((int)datvaluevalue & 0xFF));
                        temp[6] = (byte)(((int)datvaluevalue >> 8) & 0xFF);
                        temp[7] = (byte)(((int)datvaluevalue >> 16) & 0xFF);
                        temp[8] = (byte)(((int)datvaluevalue >> 24) & 0xFF);
                    }
                    catch
                    {
                        var datvaluevalue = 0;
                        Int32.TryParse(Data2Send.ToString(), out datvaluevalue);
                        temp[5] = (byte)(((int)datvaluevalue & 0xFF));
                        temp[6] = (byte)(((int)datvaluevalue >> 8) & 0xFF);
                        temp[7] = (byte)(((int)datvaluevalue >> 16) & 0xFF);
                        temp[8] = (byte)(((int)datvaluevalue >> 24) & 0xFF);
                    }
                }
            }

            //Risng up delegate , to call static function from CRC class
            ushort TempCrc = CrcInputCalc(temp.Take(9), 2);   // Delegate won  
            temp[9] = (byte)(TempCrc & 0xFF);
            temp[10] = (byte)((TempCrc >> 8) & 0xFF);

            PortChat.Send(temp);
        }
        public void OpenFromFile()
        {
            System.Windows.Forms.OpenFileDialog ChooseFile = new System.Windows.Forms.OpenFileDialog();
            ChooseFile.Filter = "All Files (*.*)|*.*";
            ChooseFile.FilterIndex = 1;

            ChooseFile.Multiselect = false;
            ChooseFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MotorController\\FirmwareUpdate\\";

            if(ChooseFile.ShowDialog() == DialogResult.OK)
            {
                _filePath = ChooseFile.FileName;
            }
        }
        #region SerialProgrammerFunctions
        public bool ini_variables()
        {
            if(String.IsNullOrWhiteSpace(MaintenanceViewModel.GetInstance.PathFW))
            {
                EventRiser.Instance.RiseEevent(string.Format("File path not valid"));
                return false;
            }
            wait = true;
            equal = 0;
            expectedResponse = new byte[] { 0 };             //  Requiered response => 139 60 134 0 0 79 218 62 0 192 135
            timeout = 0;
            if(LeftPanelViewModel._app_running)
                foundBaudrate = Rs232Interface._comPort.BaudRate;
            MaintenanceViewModel.GetInstance.PbarValueFW = 0;
            _filePath = MaintenanceViewModel.GetInstance.PathFW;

            EventRiser.Instance.RiseEevent("Burn firmware process started");
            Thread.Sleep(50);

            readText = "";
            dataFileCount = 0;

            readText = File.ReadAllText(_filePath);
            _dataFromFile.Clear();
            var _array = readText.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                for(int i = 1; i < _array.Length - 1; i++)
                    _dataFromFile.Add(Convert.ToByte(_array[i], 16));
            }
            catch(Exception) { };
            dataFileCount = _dataFromFile.Count;
            _header = new byte[22];
            _checkSum = 0;
            Array.Copy(_dataFromFile.ToArray(), 0, _header, 0, _header.Length);
            _dataFromFile.RemoveRange(0, 22);
            for(int i = 0; i < _header.Length; i++)
                _checkSum += _header[i];
            return true;
        }
        public bool connect_com()
        {
            return (autobaud_driver());
        }
        public bool autobaud_driver()
        {
            try
            {
                PortChat.GetInstance.CloseComunication();

                #region DriverAutoBaud_1[0]
                EventRiser.Instance.RiseEevent("Autobaud process start");
                _currentState = (int)eSTAGE.AUTOBAUD_DRIVER;
                foreach(var baudRate in ConnectionBase.BaudRates) //Iterate though baud rates
                {
                    foundBaudrate = baudRate;
                    PortChat.GetInstance.Main(Configuration.SelectedCom, foundBaudrate);
                    PortChat.GetInstance.ReadTick((int)(eSTATE.START));
                    PortChat.GetInstance._packetsList.Clear();
                    ParseOutputData(0, 1, 0, false, false);
                    wait = true;
                    equal = 0;
                    expectedResponse = new byte[] { 1, 0, 16, 0, 0, 0, 0, 209, 3 };
                    timeout = 0;
                    while(wait && timeout < MAX_TIMEOUT)
                    {
                        if(PortChat.GetInstance._packetsList.Count != 0)
                        {
                            for(int i = 0; i < PortChat.GetInstance._packetsList.Count(); i++)
                            {
                                for(int j = 0; j < PortChat.GetInstance._packetsList.ElementAt(i).Count(); j++)
                                {
                                    if(PortChat.GetInstance._packetsList.ElementAt(i)[j] == expectedResponse[j])
                                        equal++;
                                }
                                if(equal == expectedResponse.Length)
                                {
                                    wait = false;
                                }
                            }
                        }
                        timeout++;
                        Thread.Sleep(5);
                    }
                    if(!wait)
                        break;
                    else
                    {
                        PortChat.GetInstance.ReadTick((int)(eSTATE.STOP));
                        PortChat.GetInstance.CloseComunication();
                    }
                }
                if(timeout == MAX_TIMEOUT || wait)
                {
                    EventRiser.Instance.RiseEevent("Autobaud process fail");
                    return false;
                }
                else
                    EventRiser.Instance.RiseEevent(string.Format("Autobaud process sucess with baudrate {0}", foundBaudrate));

                #endregion DriverAutoBaud_1[0]

                EventRiser.Instance.RiseEevent(string.Format("Success opening " + Configuration.SelectedCom));
                return true;
            }
            catch(Exception)
            {
                EventRiser.Instance.RiseEevent(string.Format("Fail opening COM " + Configuration.SelectedCom));
                PortChat.GetInstance.ReadTick((int)(eSTATE.STOP));
                PortChat.GetInstance.CloseComunication();
                return false;
            }
        }
        public bool boot_command()
        {
            EventRiser.Instance.RiseEevent("Sending loader command");
            PortChat.GetInstance._packetsList.Clear();
            ParseOutputData(1, 65, 0, true, false);
            wait = true;
            equal = 0;
            expectedResponse = new byte[] { 134, 0, 0, 113, 229, 62, 0, 253, 99 };             //  Requiered response => 139 60 134 0 0 79 218 62 0 192 135
            timeout = 0;
            while(wait && timeout < MAX_TIMEOUT)
            {
                if(PortChat.GetInstance._packetsList.Count != 0)
                {
                    for(int i = 0; i < PortChat.GetInstance._packetsList.Count(); i++)
                    {
                        for(int j = 0; j < 3/*PortChat.GetInstance._packetsList.ElementAt(i).Count()*/; j++)
                        {
                            if(PortChat.GetInstance._packetsList.ElementAt(i)[j] == expectedResponse[j])
                                equal++;
                        }
                        if(equal == 3)
                        {
                            wait = false;
                        }
                    }
                }
                timeout++;
                Thread.Sleep(100);
            }
            if(timeout == MAX_TIMEOUT)
            {
                EventRiser.Instance.RiseEevent(string.Format($"Loader command fail"));
                return false;
            }
            else
                EventRiser.Instance.RiseEevent(string.Format($"Loader command sucess"));
            return true;
        }
        public bool flash_baud()
        {
            try
            {
                int flashbaud = Convert.ToInt32(MaintenanceViewModel.GetInstance.FlashBaudRate); /*(int)eBaudRate.Baud230400;*/
                PortChat.GetInstance.ReadTick((int)eSTATE.STOP);
                PortChat.GetInstance.CloseComunication();
                PortChat.GetInstance.Main(Configuration.SelectedCom, flashbaud);
                EventRiser.Instance.RiseEevent(string.Format("Success opening {0} with baudrate {1}", Configuration.SelectedCom, flashbaud));
            }
            catch(Exception)
            {
                EventRiser.Instance.RiseEevent(string.Format("Fail opening {0}", Configuration.SelectedCom));
                PortChat.GetInstance.CloseComunication();
                return false;
            }
            PortChat.GetInstance.ReadTick((int)eSTATE.START);
            return true;
        }
        public bool autobaud()
        {
            EventRiser.Instance.RiseEevent("Baud lock started");
            byte[] A = new byte[1] { 65 };
            expectedResponse = new byte[] { 65 };
            wait = true;
            for(int i = 0; i < 3 && wait == true; i++)
            {
                wait = true;
                timeout = 0;
                PortChat.GetInstance._packetsList.Clear();
                PortChat.Send(A);
                while(wait && timeout < MAX_TIMEOUT)
                {
                    if(PortChat.GetInstance._packetsList.Count != 0)
                    {
                        if(PortChat.GetInstance._packetsList.ElementAt(0)[0] == expectedResponse[0])
                        {
                            wait = false;
                            break;
                        }
                    }
                    timeout++;
                    Thread.Sleep(60);
                }
            }
            if(timeout == MAX_TIMEOUT)
            {
                EventRiser.Instance.RiseEevent("Baud lock fail");
                PortChat.GetInstance.ReadTick((int)eSTATE.STOP);
                PortChat.GetInstance.CloseComunication();
                return false;
            }
            else
                EventRiser.Instance.RiseEevent("Baud lock sucess");
            return true;
        }
        public bool erase()
        {
            Thread.Sleep(100);
            EventRiser.Instance.RiseEevent("Erase start");

            wait = true;
            equal = 0;
            timeout = 0;
            PortChat.GetInstance._packetsList.Clear();

            expectedResponse = new byte[] { (byte)(_checkSum & 0xFF), (byte)(_checkSum >> 8) };
            PortChat.Send(_header);
            while(wait && timeout < MAX_TIMEOUT)
            {
                if(PortChat.GetInstance._packetsList.Count != 0)
                {
                    for(int i = 0; i < PortChat.GetInstance._packetsList.ElementAt(0).Count(); i++)
                    {
                        if(PortChat.GetInstance._packetsList.ElementAt(0)[i] == expectedResponse[i])
                            equal++;
                    }
                    if(equal == 2)
                    {
                        wait = false;
                    }
                }
                timeout++;
                Thread.Sleep(200);
            }
            if(timeout == MAX_TIMEOUT)
            {
                EventRiser.Instance.RiseEevent(string.Format($"Erase fail"));
                PortChat.GetInstance.ReadTick((int)eSTATE.STOP);
                PortChat.GetInstance.CloseComunication();
                return false;
            }
            else
                EventRiser.Instance.RiseEevent(string.Format($"Erase sucess"));
            return true;
        }
        public bool program()
        {
            EventRiser.Instance.RiseEevent("Program start");
            bool exit = false, subExit = false;
            int tempCount = 0;

            while((_dataFromFile.ElementAt(1) << 8 | _dataFromFile.ElementAt(0)) != 0 && !exit)
            {
                int sizeBuf = ((_dataFromFile.ElementAt(1) << 8 | _dataFromFile.ElementAt(0)) + 3) * 2;
                byte[] Buf = new byte[sizeBuf];
                UInt16[] Buf16 = new UInt16[sizeBuf / 2];
                Array.Copy(_dataFromFile.ToArray(), 0, Buf, 0, sizeBuf);
                for(int i = 0; i < sizeBuf / 2; i++)
                    Buf16[i] = (UInt16)(Buf[2 * i] << 8 | Buf[2 * i + 1]);
                _dataFromFile.RemoveRange(0, sizeBuf);
                _checkSum = 0;
                for(int i = 0; i < sizeBuf; i++)
                    _checkSum += Buf[i];

                if(sizeBuf / 2 <= 1027)
                {
                    //Debug.WriteLine("checksum a {0}", _checkSum);
                    if(sizeBuf == 6)
                        continue;
                    else
                    {
                        if(!sendCS(Buf, _checkSum))
                            exit = true;
                    }
                }
                else
                {
                    subExit = false;
                    int index = 0, size = 0;
                    UInt16[] subBuf16 = new UInt16[1027];
                    Array.Copy(Buf16, index, subBuf16, 0, 1027);
                    byte[] subBuf = new byte[2054];
                    Array.Copy(Buf, index, subBuf, 0, 2054);
                    _checkSum = 0;
                    for(int i = 0; i < subBuf.Length; i++)
                        _checkSum += subBuf[i];
                    //Debug.WriteLine("checksum b {0}", _checkSum);
                    while(!subExit)
                    {
                        tempCount += size * 2;
                        MaintenanceViewModel.GetInstance.PbarValueFW = tempCount * 100 / dataFileCount;
                        //Debug.WriteLine(tempCount);
                        if(!sendCS(subBuf, _checkSum))
                            subExit = true;

                        if((1027 + ((index + 1) * 1024)) <= Buf16.Length)
                            size = 1024;
                        else
                            size = Buf16.Length - (1027 + (index * 1024));
                        if(size < 0)
                            break;
                        subBuf16 = new UInt16[size];
                        Array.Copy(Buf16, 1027 + (index * 1024), subBuf16, 0, size);
                        subBuf = new byte[size * 2];
                        Array.Copy(Buf, 2054 + (index * 2048), subBuf, 0, size * 2);
                        _checkSum = 0;
                        for(int i = 0; i < subBuf.Length; i++)
                            _checkSum += subBuf[i];
                        //Debug.WriteLine("checksum c {0}", _checkSum);
                        index++;
                    }
                    if(subExit)
                        exit = true;
                }
            }
            if(exit)
            {
                EventRiser.Instance.RiseEevent(string.Format($"Program fail"));
                return false;
            }
            else
                EventRiser.Instance.RiseEevent(string.Format($"Program sucess"));

            return true;
        }
        public void reset()
        {
            Thread.Sleep(100);
            PortChat.GetInstance._packetsList.Clear();
            byte[] restartDevice = new byte[] { 0, 0 }; // 100, 0, 0, 200, 0, 0, 0, 27, 166 };
            sendCS(restartDevice, 0, 1);
            MaintenanceViewModel.GetInstance.PbarValueFW = 100;
        }
        public bool fw_version()
        {
            EventRiser.Instance.RiseEevent("Getting FW Version");
            PortChat.GetInstance._packetsList.Clear();
            int result = 0;
            for(int j = 0; j < 3; j++)
            {
                ParseOutputData(0, 62, 3, false, false);
                wait = true;
                equal = 0;
                timeout = 0;
                int[] _id = new int[] { 0 }, _subId = new int[] { 0 };
                while(wait && timeout < MAX_TIMEOUT)
                {
                    if(PortChat.GetInstance._packetsList.Count != 0)
                    {
                        for(int i = 0; i < PortChat.GetInstance._packetsList.Count(); i++)
                        {
                            result = PortChat.GetInstance.ParseInputPacket(PortChat.GetInstance._packetsList.ElementAt(i), _id, _subId);
                            if(_id[0] == 62 && _subId[0] == 3)
                            {
                                wait = false;
                                continue;
                            }
                        }
                    }
                    timeout++;
                    Thread.Sleep(5);
                }
                if(!wait)
                    break;
            }
            if(timeout == MAX_TIMEOUT)
            {
                EventRiser.Instance.RiseEevent($"Getting FW Version fail");
                return false;
            }
            else
                EventRiser.Instance.RiseEevent("FW Version " + result);
            return true;
        }
        public bool reset_driver()
        {
            EventRiser.Instance.RiseEevent("Reset driver command");
            Thread.Sleep(100);
            PortChat.GetInstance._packetsList.Clear();
            ParseOutputData(1, 63, 9, true, false);
            wait = true;
            equal = 0;
            expectedResponse = new byte[] { 100, 0, 0, 200, 0, 0, 0, 27, 166 };
            timeout = 0;
            while(wait && timeout < MAX_TIMEOUT)
            {
                if(PortChat.GetInstance._packetsList.Count != 0)
                {
                    for(int i = 0; i < PortChat.GetInstance._packetsList.Count(); i++)
                    {
                        for(int j = 0; j < expectedResponse.Length; j++)
                        {
                            if(PortChat.GetInstance._packetsList.ElementAt(i)[j] == expectedResponse[j])
                                equal++;
                        }
                        if(equal == expectedResponse.Length)
                        {
                            wait = false;
                        }
                    }
                }
                timeout++;
                Thread.Sleep(50);
            }
            if(timeout == MAX_TIMEOUT)
            {
                EventRiser.Instance.RiseEevent(string.Format($"Reset driver fail"));
                return false;
            }
            else
                EventRiser.Instance.RiseEevent(string.Format($"Reset driver sucess"));
            return true;
        }
        public void reconnect()
        {
            PortChat.GetInstance.ReadTick((int)eSTATE.STOP);
            PortChat.GetInstance.CloseComunication();
            Rs232Interface.GetInstance.AutoConnect();
        }
        public bool exit()
        {
            MaintenanceViewModel.GetInstance.SerialProgrammerCheck = false;
            return true;
        }
        #endregion SerialProgrammerFunctions
    }

    public class PortChat
    {
        private int _index = 0;
        private int _packetIndexCounter = 0;
        byte[] readypacket = new byte[9];
        public List<byte[]> _packetsList = new List<byte[]>();

        private static readonly object Synlock = new object();
        private static PortChat _instance;
        public static PortChat GetInstance
        {
            get
            {
                lock(Synlock)
                {
                    if(_instance != null)
                        return _instance;
                    _instance = new PortChat();
                    return _instance;
                }
            }
            set
            {
                _instance = value;
            }
        }
        private PortChat()
        {
        }
        public SerialPort _serialPort = new SerialPort();
        public void Main(string serialPort, int Baudrate)
        {
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            // Create a new SerialPort object with default settings.
            //_serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = serialPort;
            _serialPort.BaudRate = Baudrate; // (int)eBaudRate.Baud230400;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 0x00000008;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;

            // Set the read/write timeouts (ms)
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
        }
        public void CloseComunication()
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }
        public static void Send(byte[] packetToSend)
        {
            //for(int i = 0; i < packetToSend.Length; i++)
            //    Debug.WriteLine("tsv {0}", packetToSend[i]);
            if(PortChat.GetInstance._serialPort != null)
                PortChat.GetInstance._serialPort.Write(packetToSend, 0, packetToSend.Length);
        }
        private Timer _receive;
        const double _receiveTickInterval = 100;

        public void ReadTick(int _mode)
        {
            switch(_mode)
            {
                case (int)eSTATE.STOP:
                    lock(this)
                    {
                        if(_receive != null)
                        {
                            lock(_receive)
                            {
                                _receive.Stop();
                                _receive.Elapsed -= Read;
                                _receive = null;
                                Thread.Sleep(10);
                            }
                        }
                    }
                    break;
                case (int)eSTATE.START:
                    if(_receive == null)
                    {
                        Task.Factory.StartNew(action: () =>
                        {
                            Thread.Sleep(100);
                            _receive = new Timer(_receiveTickInterval) { AutoReset = true };
                            _receive.Elapsed += Read;
                            _receive.Start();
                        });
                    }
                    break;
            }
        }

        public void Read(object sender, EventArgs e)
        {
            if(_serialPort != null)
            {
                byte[] buffer = new byte[_serialPort.BytesToRead];
                _serialPort.Read(buffer, 0, buffer.Length);

                if(buffer.Length > 0)
                    DescribData(buffer);
            }
        }
        public void DescribData(byte[] packet)
        {
            if(SerialProgrammer.GetInstance._currentState == (int)eSTAGE.BOOT_COMMAND)
            {
                for(int i = 0; i < packet.Length; i++)
                    FillPackets(packet[i]);
            }
            else if(SerialProgrammer.GetInstance._currentState == (int)eSTAGE.AUTOBAUD)
            {
                _packetsList.Clear();
                for(int i = 0; i < packet.Length; i++)
                    _packetsList.Add(new byte[] { packet[i] });
            }
            else if(SerialProgrammer.GetInstance._currentState == (int)eSTAGE.ERASE)
            {
                _packetsList.Clear();
                for(int i = 0; i < packet.Length; i++)
                    _packetsList.Add(new byte[] { packet[i] });
            }
            else if(SerialProgrammer.GetInstance._currentState == (int)eSTAGE.PROGRAM)
            {
                _packetsList.Clear();
                for(int i = 0; i < packet.Length; i++)
                    _packetsList.Add(new byte[] { packet[i] });
            }
            else if(SerialProgrammer.GetInstance._currentState == (int)eSTAGE.RESET)
            {
                _packetsList.Clear();
                for(int i = 0; i < packet.Length; i++)
                    FillPackets(packet[i]);
            }
            else if(SerialProgrammer.GetInstance._currentState == (int)eSTAGE.AUTOBAUD_DRIVER)
            {
                _packetsList.Clear();
                for(int i = 0; i < packet.Length; i++)
                    FillPackets(packet[i]);
            }
            else if(SerialProgrammer.GetInstance._currentState == (int)eSTAGE.FW_VERSION)
            {
                _packetsList.Clear();
                for(int i = 0; i < packet.Length; i++)
                    FillPackets(packet[i]);
            }
            else if(SerialProgrammer.GetInstance._currentState == (int)eSTAGE.RESET_DRIVER)
            {
                _packetsList.Clear();
                for(int i = 0; i < packet.Length; i++)
                    FillPackets(packet[i]);
            }
            else
            {

            }
            //for(int i = 0; i < _packetsList.Count; i++)
            //{
            //    for(int j = 0; j < _packetsList.ElementAt(i).Length; j++)
            //        Debug.WriteLine("rcv {0}", _packetsList.ElementAt(i)[j]);
            //}
        }
        private void FillPackets(byte ch)
        {
            switch(_index)
            {
                case (0):	//First magic
                    if(ch == 0x8b)
                        _index++;
                    break;
                case (1):   //Second magic
                    if(ch == 0x3c)
                        _index++;
                    else
                        _index = 0;
                    break;
                case (2):
                case (3):
                case (4):
                case (5):
                case (6):
                case (7):
                case (8):
                case (9):
                    readypacket[(_packetIndexCounter++)] = ch;
                    _index++;
                    break;
                case (10):
                    readypacket[_packetIndexCounter] = ch;
                    _packetsList.Add(readypacket);
                    _index = _packetIndexCounter = 0;
                    break;
                default:
                    _index = _packetIndexCounter = 0;
                    break;
            }
        }
        public static CrcEventhandlerCalcHostFrameCrc CrcInputCalc = CrcBase.CalcHostFrameCrc;
        public Int32 ParseInputPacket(byte[] data, int[] ID, int[] SubID)
        {
            var crclsb = data[7];
            var crcmsb = data[8];

            ushort crc = CrcInputCalc(data.Take(7), 0);

            byte[] crcBytes = BitConverter.GetBytes(crc);

            if(crcBytes[0] == crclsb && crcBytes[1] == crcmsb)//CHECK
            {
                var cmdlIdLsb = data[0];
                var cmdIdlMsb = data[1] & 0x3F;
                var subIdLsb = (data[1] >> 6) & 0x03;
                var subIdMsb = data[2] & 0x07;
                var getSet = (data[2] >> 4) & 0x01;//ASK
                var intFloat = (data[2] >> 5) & 0x01;
                var farmeColor = (data[3] >> 6) & 0x03;
                bool isInt = (intFloat == 0);//Answer Int=0/////to need check what doing!!!
                                             //Cmd ID
                int commandId = Convert.ToInt16(cmdlIdLsb);
                commandId = commandId + Convert.ToInt16(cmdIdlMsb << 8);
                ID[0] = commandId;
                //Cmd SubID
                int commandSubId = Convert.ToInt16(subIdLsb);
                commandSubId = commandSubId + Convert.ToInt16(subIdMsb << 2);
                SubID[0] = commandSubId;

                Int32 transit = data[6];
                transit <<= 8;
                transit |= data[5];
                transit <<= 8;
                transit |= data[4];
                transit <<= 8;
                transit |= data[3];

                return transit;
            }
            return 0;
        }
    }
}
