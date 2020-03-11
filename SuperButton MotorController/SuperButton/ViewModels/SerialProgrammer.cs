#define SerialProgrammerLabVIEW
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
using SuperButton.Common;
using static SuperButton.Operations;
using System.Windows.Forms;
using System.IO;
using SuperButton.Helpers;

namespace SuperButton.ViewModels
{
    enum eSTAGE
    {
        IDLE,
        BOOT_COMMAND,
        AUTOBAUD,
        HEADER,
        ERASE
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
            if(Rs232Interface._comPort != null)
            {
                Rs232Interface._comPort.DataReceived -= Rs232Interface.GetInstance.DataReceived;
                Rs232Interface._comPort.Close();
                Rs232Interface._comPort.Dispose();
            }
            try
            {
                PortChat.GetInstance.Main(Configuration.SelectedCom);
                EventRiser.Instance.RiseEevent(string.Format($"Success opening COM"));
            }
            catch(Exception)
            {
                EventRiser.Instance.RiseEevent(string.Format($"Fail opening COM"));
            }

            _currentState = (int)eSTAGE.BOOT_COMMAND;
            ParseOutputData(1, 65, 0, true, false);
            bool wait = true;
            int equal = 0;
            byte[] expectedResponse = { 134, 0, 0, 79, 218, 62, 0, 192, 135 };             //  Requiered response => 139 60 134 0 0 79 218 62 0 192 135
            int timeout = 0;
            while(wait && timeout < MAX_TIMEOUT)
            {
                if(PortChat.GetInstance._packetsList.Count != 0)
                {
                    for(int i = 0; i < PortChat.GetInstance._packetsList.ElementAt(0).Count(); i++)
                    {
                        if(PortChat.GetInstance._packetsList.ElementAt(0)[i] == expectedResponse[i])
                            equal++;
                    }
                    if(equal == 9)
                        wait = false;
                }
                timeout++;
                Thread.Sleep(100);
            }
            if(timeout == MAX_TIMEOUT)
                EventRiser.Instance.RiseEevent(string.Format($"Boot command fail"));
            else
                EventRiser.Instance.RiseEevent(string.Format($"Boot command sucess"));

            _currentState = (int)eSTAGE.AUTOBAUD;
            byte[] A = new byte[1] { 65 };
            expectedResponse = new byte[] { 65 };
            for(int i = 0; i < 5; i++)
            {
                PortChat.Send(A);

                if(PortChat.GetInstance._packetsList.Count != 0)
                {
                    if(PortChat.GetInstance._packetsList.ElementAt(0)[0] == expectedResponse[0])
                    {
                        EventRiser.Instance.RiseEevent(string.Format($"Autobaud sucess"));
                        break;
                    }
                    else
                        Thread.Sleep(100);
                }
                else
                    Thread.Sleep(100);
            }
            
            OpenFromFile();
            string readText = File.ReadAllText(_filePath);
            var _array = readText.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                for(int i = 1; i < _array.Length - 1; i++)
                    _dataFromFile.Add(Convert.ToByte(_array[i], 16) );
            }
            catch(Exception e) { };

            EventRiser.Instance.RiseEevent(string.Format($"Erase start"));
            _currentState = (int)eSTAGE.HEADER;
            byte[] _header = new byte[22];
            UInt16 _checkSum = 0;
            Array.Copy(_dataFromFile.ToArray(), 0, _header, 0, _header.Length);
            for(int i = 0; i < _header.Length; i++)
                _checkSum += _header[i];

            PortChat.GetInstance._packetsList.Clear();
            PortChat.Send(_header);
            wait = true;
            equal = 0;
            timeout = 0;
            expectedResponse = new byte[] { 0x01, 0xc5 };
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
                        wait = false;
                }
                timeout++;
                Thread.Sleep(100);
            }
            if(timeout == MAX_TIMEOUT)
                EventRiser.Instance.RiseEevent(string.Format($"Erase fail"));
            else
                EventRiser.Instance.RiseEevent(string.Format($"Erase sucess"));
            
            EventRiser.Instance.RiseEevent(string.Format($"Close COM"));
            PortChat.GetInstance._serialPort.Close();
            PortChat.GetInstance._serialPort.Dispose();
            PortChat.GetInstance.ReadTick((int)eSTATE.STOP);
            //  Rs232Interface.GetInstance.AutoConnect();
#endif
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
        public void AutoBaud()
        {

        }
        public void OpenFromFile()
        {
            System.Windows.Forms.OpenFileDialog ChooseFile = new System.Windows.Forms.OpenFileDialog();
            ChooseFile.Filter = "All Files (*.*)|*.*";
            ChooseFile.FilterIndex = 1;

            ChooseFile.Multiselect = false;
            ChooseFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MotorController\\Parameters\\";

            if(ChooseFile.ShowDialog() == DialogResult.OK)
            {
                _filePath = ChooseFile.FileName;
            }
        }
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

        public SerialPort _serialPort;

        public void Main(string serialPort)
        {
            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = serialPort;
            _serialPort.BaudRate = (int)eBaudRate.Baud230400;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 0x00000008;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();

            ReadTick((int)(eSTATE.START));
        }
        public static void Send(byte[] packetToSend)
        {
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
            else if(SerialProgrammer.GetInstance._currentState == (int)eSTAGE.HEADER)
            {
                _packetsList.Clear();
                for(int i = 0; i < packet.Length; i++)
                    _packetsList.Add(new byte[] { packet[i] });
            }
            else
            {

            }
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
    }
}
