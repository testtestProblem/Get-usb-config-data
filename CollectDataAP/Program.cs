using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CollectDataAP
{
    class Program
    {
        static Connect2UWP connect2UWP = new Connect2UWP();
        static UsbDevicesLog usbDevicesLog = new UsbDevicesLog();

        static void Main(string[] args)
        {
            string s_res;

            connect2UWP.InitializeAppServiceConnection();
            usbDevicesLog.getUSBdevicesLog();

            Console.WriteLine(UsbDevicesLog.s_USBdevicesLog);
            connect2UWP.Send2UWP(UsbDevicesLog.s_USBdevicesLog);

            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);

            System.Windows.Forms.Application.Run();

            /*
            Console.WriteLine("[1] send data to UWP");
            
            while ((s_res = Console.ReadLine()) != "")
            {
                int i_res = int.Parse(s_res);

                if (i_res == 1)
                {
                    connect2UWP.Send2UWP("Hi!", "UWP");
                }
            }*/
        }

        static public void SystemEvents_PowerModeChanged(object sender, EventArgs e)
        {
            usbDevicesLog.getUSBdevicesLog();

            Console.WriteLine(UsbDevicesLog.s_USBdevicesLog);
            connect2UWP.Send2UWP(UsbDevicesLog.s_USBdevicesLog);        
        }
    }
}
