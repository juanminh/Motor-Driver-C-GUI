//#define DEBUG_OPERATION
#define DEBUG_SET
//#define DEBUG_GET
#define New_Packet_Plot

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Abt.Controls.SciChart.Example.Data;
using MotorController.Common;
using MotorController.Data;
using MotorController.Models.DriverBlock;
using MotorController.Models.StaticClass;
using MotorController.Views;
using MotorController.Helpers;
using MotorController.ViewModels;

public struct PacketFields
{
    public object Data2Send;
    public Int16 ID;
    public Int16 SubID;
    public bool IsSet;
    public bool IsFloat;
}

//Inter connection between CRC and Parser classes performed by using simple delegates
public delegate ushort CrcEventhandlerCalcHostFrameCrc(IEnumerable<byte> data, int offset);

namespace MotorController.Models.ParserBlock
{
    internal delegate void Parser2SendHandler(object sender, Parser2SendEventArgs e);//Event declaration, when parser will finish operation. Rise event
    class ParserRayonM1
    {
        private static readonly object Synlock = new object();             //Singletone variable
        public static readonly object PlotListLock = new object();             //Singletone variable
        private static ParserRayonM1 _parserRayonM1instance;               //Singletone variable
        public event Parser2SendHandler Parser2Plot;
        //private Thread decodeThread;
        private bool stop = false;
        private double _deltaTOneChen = 0;
        private DoubleSeries datasource1 = new DoubleSeries();
        public static ManualResetEvent mre = new ManualResetEvent(false);
        private float iqFactor = (float)Math.Pow(2.0, -15);
        //private int IntegerFactor = 1;

        public event Parser2SendHandler Parser2Send;
        public bool StopParser { get { return stop; } set { stop = value; } }

        //Simple delegate. calls static function from Crc class
        public static CrcEventhandlerCalcHostFrameCrc CrcInputCalc = CrcBase.CalcHostFrameCrc;

        public double TimeIntervalChannel1 = 0;
        public DoubleSeries DsCh1 = new DoubleSeries();
        public List<DoubleSeries> PlotDatalistList = new List<DoubleSeries>();

        //public Queue<double> FifoplotList = new Queue<double>();
        public ConcurrentQueue<float> FifoplotList = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotListCh2 = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotListCh3 = new ConcurrentQueue<float>();
        public ConcurrentQueue<float> FifoplotListCh4 = new ConcurrentQueue<float>();
        public UInt32 RefreshCounter = 0;
        public UInt32 Ticker = 0;
        public UInt32 TickerC = 1;

        private List<int> exceptionID = new List<int>(); // Contains all the ID that dont need to be descripted by refersh manager class.
        int[] exceptionID_Arr = { 100, 67, 34, 35, 36, 65 }; // 100: Error, 67: Load To/From file params started, 34, 35, 36: Init plots table.
        public ParserRayonM1()
        {
            Rs232Interface.GetInstance.RxtoParser += parseOutdata;
            //Rs232Interface.GetInstance.TxtoParser += parseIndata;
            Packetizer packetizer = new Packetizer();

            foreach(var element in exceptionID_Arr)
                exceptionID.Add(element);
        }
        #region Parser_Selection

        //TODO here will switch between parsers depends on sender object
#if DEBUG_OPERATION
        //int CurrentCmdCounterTx = 0;
#endif
        public void parseOutdata(object sender, Rs232InterfaceEventArgs e)
        {
#if DEBUG_OPERATION
            if(e.PacketRx.ID == DebugOutput.GetInstance.ID && e.PacketRx.SubID == DebugOutput.GetInstance.subID && e.PacketRx.IsSet == true)
            {
                CurrentCmdCounterTx++;
                Debug.WriteLine("CurrentCmdCounterTx: " + CurrentCmdCounterTx + " Value: " + e.PacketRx.Data2Send);
            }
#endif
            if(sender is Rs232Interface)//RayonM3 Parser
            {
                ParseOutputData(e.PacketRx.Data2Send, e.PacketRx.ID, e.PacketRx.SubID, e.PacketRx.IsSet,
                    e.PacketRx.IsFloat);
#if DEBUG_OPERATION
                //Debug.WriteLine("{0} {1}[{2}]={3} {4}.", e.PacketRx.IsSet ? "Set" : "Get", e.PacketRx.ID, e.PacketRx.SubID, e.PacketRx.Data2Send, e.PacketRx.IsFloat ? "F" : "I");
                if(e.PacketRx.ID == DebugOutput.GetInstance.ID && e.PacketRx.SubID == DebugOutput.GetInstance.subID && e.PacketRx.IsSet == true)
                {
                    //CurrentCmdCounterTx++;
                    Debug.WriteLine(" Value sent: " + e.PacketRx.Data2Send);
                }
#endif
                if(LeftPanelViewModel.GetInstance != null)
                { // perform Get after "set" function
                    if(LeftPanelViewModel._app_running && !DebugViewModel.GetInstance.EnRefresh && e.PacketRx.IsSet != false)
                    {
                        Thread.Sleep(1);
                        if(e.PacketRx.ID != 63 && e.PacketRx.ID != 67)
                        {
                            ParseOutputData(/*e.PacketRx.Data2Send*/"", e.PacketRx.ID, e.PacketRx.SubID, false,
                            e.PacketRx.IsFloat);
                        }
                    }
                }
            }//Add Here aditional parsers...
        }

        public void parseIndata(object sender, Rs232InterfaceEventArgs e)
        {

            if(sender is Rs232Interface)//RayonM3 Parser
            {
                ParseInputData(e.ParseLength, e.InputChank);
            }//Add Here aditional parsers...
        }

        #endregion
        #region RayonM2_Parser

        //TODO add try/catch

        #region Input_Parse 
        //RayonRs232 old parser
        public void ParseInputData(int length, byte[] dataInput)
        {
            if(Rs232Interface.GetInstance.IsSynced == false)//TODO
            {
                Rs232Interface.GetInstance.IsSynced = true;
            }
            else
            {
                //Parser
                int PlotDataSampleLSB;
                int PlotDataSampleMSB;
                int PlotDataSample;
                int i = 0;
                //int limit = (dataInput.Length - (dataInput.Length%12));

                for(; i < dataInput.Length - 24;)
                {

                    if(dataInput[i] == 0xbb && dataInput[i + 1] == 0xcc)
                    {

                        XYPoint xyPoint1 = new XYPoint();
                        XYPoint xyPoint2 = new XYPoint();
                        XYPoint xyPoint3 = new XYPoint();
                        XYPoint xyPoint4 = new XYPoint();
                        XYPoint xyPoint5 = new XYPoint();

                        //First Sample

                        PlotDataSampleLSB = (short)dataInput[i + 2];
                        PlotDataSampleMSB = (short)dataInput[i + 3];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        xyPoint1.Y = (double)PlotDataSample;
                        xyPoint1.X = _deltaTOneChen;

                        _deltaTOneChen += 0.1;

                        datasource1.Add(xyPoint1);

                        //Second
                        PlotDataSampleLSB = (short)dataInput[i + 4];
                        PlotDataSampleMSB = (short)dataInput[i + 5];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        xyPoint2.Y = (double)PlotDataSample;
                        xyPoint2.X = _deltaTOneChen;

                        _deltaTOneChen += 0.1;

                        datasource1.Add(xyPoint2);

                        //Third Sample
                        PlotDataSampleLSB = (short)dataInput[i + 6];
                        PlotDataSampleMSB = (short)dataInput[i + 7];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        xyPoint3.Y = (double)PlotDataSample;
                        xyPoint3.X = _deltaTOneChen;

                        _deltaTOneChen += 0.1;

                        datasource1.Add(xyPoint3);

                        //Fourth sample
                        PlotDataSampleLSB = (short)dataInput[i + 8];
                        PlotDataSampleMSB = (short)dataInput[i + 9];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        xyPoint4.Y = (double)PlotDataSample;
                        xyPoint4.X = _deltaTOneChen;

                        datasource1.Add(xyPoint4);

                        _deltaTOneChen += 0.1;

                        //Fifth sample
                        PlotDataSampleLSB = (short)dataInput[i + 10];
                        PlotDataSampleMSB = (short)dataInput[i + 11];
                        PlotDataSample = (PlotDataSampleMSB << 8) | PlotDataSampleLSB;

                        if(PlotDataSample != 4.0)
                        {
                            // int a = 5;
                        }

                        xyPoint5.Y = (double)PlotDataSample;
                        xyPoint5.X = _deltaTOneChen;

                        _deltaTOneChen += 0.1;

                        datasource1.Add(xyPoint5);

                        i = i + 11;
                    }
                    i++;
                }
                Parser2Plot(this, new Parser2SendEventArgs(datasource1));
                datasource1.Clear();
            }
        }

        #endregion //TODO 

        #region Output_Parse

        //Send data to controller
        public void ParseOutputData(object Data2Send, Int16 Id, Int16 SubId, bool IsSet, bool IsFloat)
        {
#if(DEBUG && DEBUG_OPERATION)
#if DEBUG_SET
            if(IsSet && Id != 64)
            {
                Debug.Write(DateTime.Now.ToString("hh.mm.ss.ffffff"));
                Debug.WriteLine("{0} {1}[{2}]={3} {4}.", IsSet ? "Set" : "Get", Id, SubId, Data2Send, IsFloat ? "F" : "I");
            }
#endif
#if DEBUG_GET
            Debug.WriteLine("{0} {1}[{2}]={3} {4}.", IsSet ? "Set" : "Get", Id, SubId, Data2Send, IsFloat ? "F" : "I");
#endif
#endif
            byte[] temp;
            if(IsSet)
                temp = new byte[11] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            else
                temp = new byte[7] { 0, 0, 0, 0, 0, 0, 0 };

            char tempChar = (char)0;

            temp[0] = 0x49;                  //PreambleLSByte
            temp[1] = 0x5d;                   //PreambleMsbyte
            temp[2] = (byte)(Id);       // ID msb

            tempChar = (char)(tempChar | ((char)(((Id >> 8)) & 0x3F)) | (((char)(SubId & 0x3)) << 6));

            temp[3] = (byte)tempChar;

            tempChar = (char)0;

            tempChar = (char)(tempChar | (char)(SubId >> 2));

            temp[4] = (byte)tempChar;

            if(IsSet == false)                      //Set/Get
            {
                temp[4] |= (1 << 4);
            }

            //if (Data2Send is Double)        //Float/Int
            //{
            //    temp[4] |= (1<<5);  
            //}
            if(IsFloat)        //Float/Int
            {
                temp[4] |= (1 << 5);
            }

            // 1<<6  -- Color 1
            // 1<<7  -- Color 2

            if(IsSet == false)
            {
                //Risng up delegate , to call static function from CRC class
                ushort TempGetCrc = CrcInputCalc(temp.Take(5), 2);
                temp[5] = (byte)(TempGetCrc & 0xFF);
                temp[6] = (byte)((TempGetCrc >> 8) & 0xFF);
                if(Parser2Send != null)
                {
                    Parser2Send(this, new Parser2SendEventArgs(temp));
                }
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
            else
            if(Data2Send is int)//Data int
            {

                //Int32 transit =(Int32) Data2Send;
                temp[5] = (byte)(((int)Data2Send & 0xFF));
                temp[6] = (byte)(((int)Data2Send >> 8) & 0xFF);
                temp[7] = (byte)(((int)Data2Send >> 16) & 0xFF);
                temp[8] = (byte)(((int)Data2Send >> 24) & 0xFF);
            }
            else
            //my fix
            if(IsFloat)                                           //Data float
            {
                float value = 0;
                float.TryParse((string)Data2Send, out value);
                byte[] _value = BitConverter.GetBytes(value);
                temp[5] = (byte)(_value[0]);
                temp[6] = (byte)(_value[1]);
                temp[7] = (byte)(_value[2]);
                temp[8] = (byte)(_value[3]);

                //var datvaluevalue = BitConverter.GetBytes((float)(float.Parse((string)Data2Send)));
                //float newPropertyValuef = System.BitConverter.ToSingle(datvaluevalue, 0);
                //temp[5] = (byte)(datvaluevalue[0]);
                //temp[6] = (byte)(datvaluevalue[1]);
                //temp[7] = (byte)(datvaluevalue[2]);
                //temp[8] = (byte)(datvaluevalue[3]);
            }
            else // String Value
            {
                if(Data2Send.ToString().Length != 0)
                {
                    if(Data2Send.ToString().IndexOf(".") != -1)
                    {
                        Data2Send = Data2Send.ToString().Substring(0, Data2Send.ToString().IndexOf("."));
                    }
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

            //crcTask.Start();
            // ushort TempCrc = crcTask.Result;

            temp[9] = (byte)(TempCrc & 0xFF);
            temp[10] = (byte)((TempCrc >> 8) & 0xFF);

            //Rise another event that sends out to target

#if DEBUG_OPERATION
            if(Id == DebugOutput.GetInstance.ID && SubId == DebugOutput.GetInstance.subID && IsSet == true)
            {
                Debug.WriteLine("Parser2Send data: " + Data2Send);
            }
#endif
            if(Parser2Send != null)
            {
                Parser2Send(this, new Parser2SendEventArgs(temp));
            }


        }
        #endregion

        #endregion

        public void ParseSynchAcktData(List<byte[]> dataList)
        {
            for(int i = 0; i < dataList.Count; i++)
            {
                ParsesynchAckMessege(dataList[i]);
            }
        }
        private void ParsesynchAckMessege(byte[] data) // Autobaud Check Response.
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
                //Cmd SubID
                int commandSubId = Convert.ToInt16(subIdLsb);
                commandSubId = commandSubId + Convert.ToInt16(subIdMsb << 2);

                UInt32 transit = data[6];
                transit <<= 8;
                transit |= data[5];
                transit <<= 8;
                transit |= data[4];
                transit <<= 8;
                transit |= data[3];
                //Debug.WriteLine("Synch: " + transit.ToString());
                // if autobaud command is SYNCH 64[0] || 64[1] so transit will be 0x8B3C8B3C else 0
                //if(transit == 0x8B3C8B3C) // 1
                //    Rs232Interface.GetInstance.IsSynced = true;
                //else if(transit == 0 && commandId == 1 && commandSubId == 0)
                //    Rs232Interface.GetInstance.IsSynced = true;
                //else 
                try
                {
                    if(commandId == 61 && commandSubId == 1 && Rs232Interface._comPort.BaudRate == ConnectionBase.BaudRates[transit - 1])
                        Rs232Interface.GetInstance.IsSynced = true;
                    else
                        Rs232Interface.GetInstance.IsSynced = false;
                }
                catch { }
                //Debug.WriteLine("Receive: " + transit.ToString());
                //Debug.WriteLine("Baudrate Receive: " + Rs232Interface._comPort.BaudRate.ToString());
                mre.Set();
            }
        }

        public static byte[] DebugData = { };
        public bool ParseInputPacket(byte[] data)
        {
            DebugData = data;
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
                //Cmd SubID
                int commandSubId = Convert.ToInt16(subIdLsb);
                commandSubId = commandSubId + Convert.ToInt16(subIdMsb << 2);
                //int newPropertyValueInt=0;
                float newPropertyValuef = 0;
                Int32 transit = data[6];
                transit <<= 8;
                transit |= data[5];
                transit <<= 8;
                transit |= data[4];
                transit <<= 8;
                transit |= data[3];

                var dataAray = new byte[4];
                Array.Copy(data, 3, dataAray, 0, 4);

                newPropertyValuef = BitConverter.ToSingle(dataAray, 0);

                if(WizardWindowViewModel.GetInstance.send_update_parameters && getSet == 0 && commandId != 64)
                {
                    DataViewModel myValue;
                    if(WizardWindowViewModel.GetInstance.OperationList.TryGetValue(new Tuple<int, int>(commandId, commandSubId), out myValue))
                    {
                        WizardWindowViewModel.GetInstance.send_operation_count++;
                        WizardWindowViewModel.operation_echo.Remove(new Tuple<int, int>(commandId, commandSubId));
                        //Debug.WriteLine("Op Removed: {0}[{1}]={2} {3} {4}.", commandId, commandSubId, transit, "I", getSet == 0 ? "Set" : "Get");
                    }
                }

                if(LeftPanelViewModel.GetInstance.StarterPlotFlag || commandId == 34 || commandId == 35 || commandId == 36) // build plot list
                {
                    OscilloscopeParameters.plot_transfert(commandId, commandSubId, getSet, transit, data);
                }
                else if(LeftPanelViewModel.GetInstance.StarterOperationFlag &&
                    !exceptionID.Contains(commandId) ||
                    !exceptionID.Contains(commandId) ||
                    ParametarsWindowViewModel.TabControlIndex == (int)eTab.DEBUG)
                {
                    RefreshManager.GetInstance.UpdateModel(new Tuple<int, int>(commandId, commandSubId), isInt ? transit.ToString() : newPropertyValuef.ToString(), isInt);
#if(DEBUG && DEBUG_OPERATION)
                    if(getSet == 0 && commandId != 64)
                    {
                        Debug.Write(DateTime.Now.ToString("hh.mm.ss.ffffff"));
                        Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, isInt ? transit.ToString() : newPropertyValuef.ToString(), "I", getSet == 0 ? "Set" : "Get");
                    }
#endif

#if OLD
                    if(isInt)
                    {
                        //if(getSet == 1)
                            RefreshManager.GetInstance.UpdateModel(new Tuple<int, int>(commandId, commandSubId), transit.ToString(), true);
                        //else if(ParametarsWindowViewModel.TabControlIndex == (int)eTab.DEBUG)
                        //    RefreshManger.GetInstance.UpdateModel(new Tuple<int, int>(commandId, commandSubId), newPropertyValuef.ToString(), isInt);
#if(DEBUG && DEBUG_OPERATION)
#if DEBUG_SET
                        if(getSet == 0)
                            Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, transit, "I", getSet == 0 ? "Set" : "Get");
#endif
#if DEBUG_GET
                        if(getSet == 1)
                            Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, transit, "I", getSet == 0 ? "Set" : "Get");
#endif
#endif
                    }
                    else
                    {
                        
                        //if(getSet == 1)
                            RefreshManager.GetInstance.UpdateModel(new Tuple<int, int>(commandId, commandSubId), newPropertyValuef.ToString(), false);
                        //else if(ParametarsWindowViewModel.TabControlIndex == (int)eTab.DEBUG)
                        //    RefreshManger.GetInstance.UpdateModel(new Tuple<int, int>(commandId, commandSubId), newPropertyValuef.ToString(), isInt);

#if(DEBUG && DEBUG_OPERATION)
#if DEBUG_SET
                        if(getSet == 0)
                            Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, newPropertyValuef, "F", getSet == 0 ? "Set" : "Get");
#endif
#if DEBUG_GET
                        if(getSet == 1)
                            Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, newPropertyValuef, "F", getSet == 0 ? "Set" : "Get");
#endif
#endif
                    }
#endif
                }
                else if(commandId == 67) // Save driver parameters to file, Load parameters to driver from file
                {
#if(DEBUG && DEBUG_OPERATION)
                    Debug.WriteLine("{0} {1}[{2}]={3} {4} {5}.", "Drv", commandId, commandSubId, transit, "I", getSet == 0 ? "Set" : "Get");
#endif
                    MaintenanceViewModel.GetInstance.data_transfert(commandId, commandSubId, getSet, transit);
                }
                else
                {   // Error ID 100
                    Commands.GetInstance.driver_error_occured(commandId, commandSubId, transit);
                }
                return true;
            }
            Debug.WriteLine("return false");
            return false;
        }
        public static ParserRayonM1 GetInstanceofParser
        {
            get
            {
                if(_parserRayonM1instance != null)
                    return _parserRayonM1instance;
                lock(Synlock)
                {
                    _parserRayonM1instance = new ParserRayonM1();
                    return _parserRayonM1instance;
                }
            }
        }

        public void ParsePlot(List<byte[]> PlotList)
        {
            // In order to achive best performance using good old-fashioned for loop: twice faster! then "foreach (byte[] packet in PlotList)"
            //Debug.WriteLine("ParsePlot 1" + DateTime.Now.ToString("h:mm:ss.fff"));
            List<string> _ch_plot_type = new List<string>();
            try
            {
                for(int j = 0; j < Commands.GetInstance.GenericCommandsGroup["ChannelsList"].Count; j++)
                {
                    _ch_plot_type.Add(((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsList[
                        new Tuple<int, int>(
                            ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][j]).CommandId),
                            ((int)((UC_ChannelViewModel)Commands.GetInstance.GenericCommandsGroup["ChannelsList"][j]).CommandSubId))]).PlotType);
                }
            }
            catch { }
            for(var i = 0; i < PlotList.Count; i++)
            {
                lock(PlotListLock)
                {
                    #region New_Packet_Plot
                    if(OscilloscopeParameters.ChanTotalCounter == 1)
                    {
                        if(_ch_plot_type.Contains("Int32") || _ch_plot_type.Contains("Float32"))
                        {
                            var element = ((PlotList[i][5] << 24) | (PlotList[i][4] << 16) | (PlotList[i][3] << 8) | (PlotList[i][2]));
                            //First
                            FifoplotList.Enqueue(element);
                            //Second
                            FifoplotList.Enqueue(element);

                            element = ((PlotList[i][9] << 24) | (PlotList[i][8] << 16) | (PlotList[i][7] << 8) | (PlotList[i][6]));
                            //Third
                            FifoplotList.Enqueue(element);
                            //Fourth
                            FifoplotList.Enqueue(element);
                        }
                        else
                        {
                            //First
                            FifoplotList.Enqueue((short)((PlotList[i][3] << 8) | PlotList[i][2]));
                            //Second
                            FifoplotList.Enqueue((short)((PlotList[i][5] << 8) | PlotList[i][4]));
                            //Third
                            FifoplotList.Enqueue((short)((PlotList[i][7] << 8) | PlotList[i][6]));
                            //Fourth
                            FifoplotList.Enqueue((short)((PlotList[i][9] << 8) | PlotList[i][8]));
                        }
                    }
                    else if(OscilloscopeParameters.ChanTotalCounter == 2)
                    {
                        if(_ch_plot_type.ElementAt(0) == "Int32" || _ch_plot_type.ElementAt(0) == "Float32")
                        {
                            var element = ((PlotList[i][5] << 24) | (PlotList[i][4] << 16) | (PlotList[i][3] << 8) | (PlotList[i][2]));
                            //First
                            FifoplotList.Enqueue(element);
                            //Second
                            FifoplotList.Enqueue(element);
                        }
                        else
                        {
                            //First
                            FifoplotList.Enqueue((short)((PlotList[i][3] << 8) | PlotList[i][2]));
                            //Second
                            FifoplotList.Enqueue((short)((PlotList[i][5] << 8) | PlotList[i][4]));
                        }
                        if(_ch_plot_type.ElementAt(1) == "Int32" || _ch_plot_type.ElementAt(1) == "Float32")
                        {
                            var element = ((PlotList[i][9] << 24) | (PlotList[i][8] << 16) | (PlotList[i][7] << 8) | (PlotList[i][6]));
                            //Third
                            FifoplotListCh2.Enqueue(element);
                            //Fourth
                            FifoplotListCh2.Enqueue(element);
                        }
                        else
                        {
                            //Third
                            FifoplotListCh2.Enqueue((short)((PlotList[i][7] << 8) | PlotList[i][6]));
                            //Fourth
                            FifoplotListCh2.Enqueue((short)((PlotList[i][9] << 8) | PlotList[i][8]));
                        }
                    }
                    #endregion New_Packet_Plot
                }
            }
            PlotList.Clear();
        }
    }//Class
}//NameSpace


