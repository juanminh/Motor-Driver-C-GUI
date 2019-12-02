﻿using SuperButton.Models.DriverBlock;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media.Animation;

namespace SuperButton.Views
{
    public static class OscilloscopeParameters
    {
        public static List<Tuple<float, float>> ScaleAndGainList = new List<Tuple<float, float>>();
        public static List<Int32> plotGeneral = new List<Int32>();
        public static List<float> plotFullScale = new List<float>();
        public static float SingleChanelFreqC = (float)6666.666667;
        public static float ChanelFreq = SingleChanelFreqC;
        public static float Step = 1 / SingleChanelFreqC;

        public static float IfullScale = (float)192.0; // old 576.0
        public static float VfullScale = (float)68.0;

        public static float FullScale;
        public static float Gain;
        public static float FullScale2;
        public static float Gain2;

        public static int ChanTotalCounter = 0;

        public static int plotCount = 0;
        public static int plotCount_temp = 0;

        static string[] plotName = new[] {
            "None",
            "Motor Current",
            "I Phase A",
            "I Phase B",
            "I Phase C",
            "IRms",
            "Filtered Irms",
            "I PSU",
            "BEMF Phase A",
            "BEMF Phase B",
            "BEMF Phase C",
            "VDC motor",
            "VDC 12v",
            "VDC 5v",
            "VDC 3v",
            "VDC Ref",
            "Analog Command",
            "Sin Analog Enc",
            "Cos Analog Enc",
            "Hall Mech Angle",
            "Qep1 Mech Angle",
            "Qep2 Mech Angle",
            "SSI Mech Angle",
            "Sin Cos Mech Angle",
            "Com Mech Angle",
            "Commutation Angle",
            "HALL Speed",
            "HALL Elect Angle",
            "HALL Position",
            "Enc1 Speed",
            "Enc1 Elect Angle",
            "Enc1 Position",
            "Enc2 Speed",
            "Enc2 Elect Angle",
            "Enc2 Position",
            "Sensorless Speed",
            "Sensorless Elect Angle",
            "SinCosAngle",
            "delta Hall Enc",
            "Current Cmd",
            "Speed Cmd",
            "Position Cmd",
            "Current Iq Fdb",
            "Current Iq Ref",
            "Current Iq Err",
            "Current Id Fdb",
            "Current Id Ref",
            "Current Id Err",
            "Speed Fdb",
            "Speed Ref",
            "Speed Fdb LPF",
            "Speed Err",
            "Position Fdb",
            "Position Ref",
            "Position Err",
            "Digital In1",
            "Digital In2",
            "Digital In3",
            "Digital In4",
            "Digital In5",
            "Digital In6",
            "Digital In7",
            "Digital In8",
            "Digital Out1",
            "Digital Out2",
            "Digital Out3",
            "Digital Out4",
            "Digital Out5",
            "Digital Out6",
            "Digital Out7",
            "Digital Out8",
            "cla Debug1",
            "cla Debug2",
            "Test Signal1",
            "Test Signal2",
            "Test Signal3",
            "Test Signal4"

        };
        static List<string> plotName_ls = new List<string>();
        public static List<string> plotType_ls = new List<string>();

        static string[] plotType = new[] { "Integer", "Float", "Iq24", "Iq15" };
        static string[] plotUnit = new[] { "Amper", "Volt", "", "", "", "Elec Angle", "mechanical Angle", "", "", "", "RPM Per Volt", "Count Per Sec", "Round Per Minute", "Counts" };

        static OscilloscopeParameters()
        {
            for(int i = 0; i < plotName.Length; i++)
                plotName_ls.Add(plotName[i]);
        }

        public static void fillPlotList()
        {
            if(plotCount == plotCount_temp)
            {
                plotGeneral.Clear();
                plotFullScale.Clear();
                plotType_ls.Clear();
                OscilloscopeViewModel.GetInstance.Channel1SourceItems.Clear();
                OscilloscopeViewModel.GetInstance.Channel2SourceItems.Clear();
                OscilloscopeViewModel.GetInstance.ChannelYtitles.Clear();
                ScaleAndGainList.Clear();
            }

            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = "",
                ID = 35,
                SubID = Convert.ToInt16(plotCount - plotCount_temp + 1),
                IsSet = false,
                IsFloat = false
            });
            Thread.Sleep(5);
            Rs232Interface.GetInstance.SendToParser(new PacketFields
            {
                Data2Send = "",
                ID = 36,
                SubID = Convert.ToInt16(plotCount - plotCount_temp + 1),
                IsSet = false,
                IsFloat = false
            });
            Thread.Sleep(5);

            plotCount_temp--;
            if(plotCount_temp == 0)
            {
                buildPlotList();
            }
        }
        private static void buildPlotList()
        {
            int i = 0;
            OscilloscopeViewModel.GetInstance.Channel1SourceItems.Add("Pause");
            OscilloscopeViewModel.GetInstance.ChannelYtitles.Add("Pause", "");
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, (float)1.0)); //Pause
            plotType_ls.Add(plotType[0]); //Pause
            foreach(var element in plotGeneral)
            {
                if(element > 0)
                {
                    OscilloscopeViewModel.GetInstance.Channel1SourceItems.Add(plotName_ls[element & 0xFFFF]);
                    OscilloscopeViewModel.GetInstance.ChannelYtitles.Add(plotName_ls[element & 0xFFFF], plotUnit[(element >> 24) & 0xFF]);
                    plotType_ls.Add(plotType[(element >> 16) & 0xFF]);
                    ScaleAndGainList.Add(new Tuple<float, float>(1, plotFullScale[i]));
                }
                i++;
            }
        }
        public static void InitList()
        {

            //Init list
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, (float)1.0)); //Pause            
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, IfullScale));//IqFeedback       
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, IfullScale));//I_PhaseA         
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, IfullScale));//I_PhaseB         
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, IfullScale));//I_PhaseC
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, VfullScale));//VDC_Motor // 2.0
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, VfullScale));//BEMF_PhaseA
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, VfullScale));//BEMF_PhaseB
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, VfullScale));//BEMF_PhaseC
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, 1));//HALL_LPF_Speed
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, 1));//HALL_Elect_Angle
            ScaleAndGainList.Add(new Tuple<float, float>((float)1.0, 1));//QEP1_LPF_Speed
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//QEP1_Elect_Angle
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//QEP2_LPF_Speed
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//QEP2_Elect_Angle
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//SSI_LPF_Speed
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//SSI_Elect_Angle
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//SL_Elect_Angle
            ScaleAndGainList.Add(new Tuple<float, float>(1, IfullScale));//IRms
            ScaleAndGainList.Add(new Tuple<float, float>(1, IfullScale));//IRms(filterd)
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//SL_LPF_Speed
            ScaleAndGainList.Add(new Tuple<float, float>(1, 360));//CommutationAngle
            ScaleAndGainList.Add(new Tuple<float, float>(1, (float)Math.Pow(2, 15)));//PositionFdb
            ScaleAndGainList.Add(new Tuple<float, float>(1, (float)Math.Pow(2, 15)));//PositionRef
            ScaleAndGainList.Add(new Tuple<float, float>((float)1, (float)Math.Pow(2, 15)));//Test_Signal
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//Cla_filt0
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//Cmd_Ref
            ScaleAndGainList.Add(new Tuple<float, float>(1, 1));//Cmd_Ref_filt

            ScaleAndGainList.Add(new Tuple<float, float>(1, (float)Math.Pow(2, 15)));          // SinEnc
            ScaleAndGainList.Add(new Tuple<float, float>(1, (float)Math.Pow(2, 15)));          // CosEnc
            ScaleAndGainList.Add(new Tuple<float, float>(1, (float)Math.Pow(2, 15)));          // InterAngle
            ScaleAndGainList.Add(new Tuple<float, float>(1, (float)Math.Pow(2, 15)));          // SpeedRefPI
            ScaleAndGainList.Add(new Tuple<float, float>(1, (float)Math.Pow(2, 15)));          // SpeedFdb
            ScaleAndGainList.Add(new Tuple<float, float>(1, IfullScale)); // CurrentRefPI

            OscilloscopeViewModel.GetInstance.FillDictionary();
        }
    }

}