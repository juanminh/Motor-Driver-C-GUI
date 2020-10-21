using System;
using System.Diagnostics;
using Abt.Controls.SciChart.Example.Data;
using MotorController.Data;
using MotorController.ViewModels;
using MotorController.Helpers;
using System.Threading;

namespace MotorController.Models.DriverBlock
{
    public class Rs232InterfaceEventArgs : EventArgs
    {
        public readonly PacketFields PacketRx;
        public readonly int ParseLength;
        public readonly byte[] InputChank;
        public readonly string ConnecteButtonLabel;

       // public readonly DoubleSeries Datasource1;
        public byte[] DataChunk { get; private set; }
        public readonly byte SMagicFirst;
        public readonly byte SMagicSecond;
        public readonly byte PMagicFirst;
        public readonly byte PMagicSecond;
        public readonly UInt16 PacketLength;
        public Rs232InterfaceEventArgs(byte[] dataChunk, byte smagicFirst, byte smagicSecond, byte pmagicFirst, byte pmagicSecond, UInt16 packetLength)
        {
            DataChunk = dataChunk;
            SMagicFirst = smagicFirst;
            SMagicSecond = smagicSecond;
            PacketLength = packetLength;
            PMagicFirst = pmagicFirst;
            PMagicSecond = pmagicSecond;
        }

        
        public Rs232InterfaceEventArgs(byte[] dataChunk)
        {
            LeftPanelViewModel.GetInstance.led = LeftPanelViewModel.RX_LED;
            DataChunk = dataChunk;      //Receive packet
        }

        public Rs232InterfaceEventArgs(string connecteButtonLabel)
        {
            ConnecteButtonLabel = connecteButtonLabel;
        }
        
        public Rs232InterfaceEventArgs(PacketFields packetRx)
        {
            LeftPanelViewModel.GetInstance.led = LeftPanelViewModel.TX_LED;
            PacketRx = packetRx;     // Send Packet
        }
    }
}