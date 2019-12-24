using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperButton.Helpers
{
    public sealed class EventRiser
    {
        public event EventHandler LoggerEvent;
        public event EventHandler LedEventTx;
        public event EventHandler LedEventRx;


        static readonly EventRiser _instance = new EventRiser();
        public static EventRiser Instance
        {
            get
            {
                if (_instance == null)
                    return new EventRiser();
                return _instance;
            }

        }
        int logCounter = 0;
        public void RiseEevent(string msg)
        {
            LoggerEvent(null, new CustomEventArgs() { Msg = DateTime.Now.ToString("mm:ss - ") + msg }); // DateTime.Now.ToString("mm:ss - ") + 
            //Debug.WriteLine("Event catch");
        }
        public void RiseEventLedTx(int led)
        {
           LedEventTx(null, new CustomEventArgs() { LedTx = led });
        }
        public void RiseEventLedRx(int led)
        {
            LedEventRx(null, new CustomEventArgs() { LedRx = led });
        }

    }

    public class CustomEventArgs : EventArgs
    {
        public string Msg { get; set; }
        public int LedTx { get; set; }
        public int LedRx { get; set; }

    }
}
