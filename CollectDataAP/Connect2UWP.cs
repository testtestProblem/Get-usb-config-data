using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace CollectDataAP
{
    class Connect2UWP
    {
        private AppServiceConnection connection = null;
        /// <summary>
        /// Open connection to UWP app service
        /// </summary>
        public async void InitializeAppServiceConnection()
        {
            connection = new AppServiceConnection();
            connection.AppServiceName = "SampleInteropService";
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                //In console, something wrong
                Console.WriteLine(status.ToString());
                //In app, something went wrong ...
                //MessageBox.Show(status.ToString());
                //this.IsEnabled = false;
            }
        }

        /// <summary>
        /// Handles the event when the desktop process receives a request from the UWP app
        /// </summary>
        //static private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        //{
        //    // retrive the reg key name from the ValueSet in the request
        //    /*
        //    string key = args.Request.Message["KEY"] as string;
        //    if (key == "abcd")
        //    {
        //        // compose the response as ValueSet
        //        ValueSet response = new ValueSet();
        //        response.Add("GOOD", "KEY IS FOUNDED ^^");

        //        // send the response back to the UWP
        //        await args.Request.SendResponseAsync(response);
        //    }
        //    else
        //    {
        //        ValueSet response = new ValueSet();
        //        response.Add("ERROR", "INVALID REQUEST");
        //        await args.Request.SendResponseAsync(response);
        //    }
        //    */
        //    int? key1 = args.Request.Message["KEY1"] as int?;
        //    int? key2 = args.Request.Message["KEY2"] as int?;

        //    if (key1 != null && key2 != null)
        //    {
        //        int ans = (int)key1 + (int)key2;

        //        // compose the response as ValueSet
        //        ValueSet response = new ValueSet();
        //        response.Add("KEY3", ans);

        //        // send the response back to the UWP
        //        await args.Request.SendResponseAsync(response);
        //    }
        //}

        //multiple key

        /// <summary>
        /// Handles the event when the desktop process receives a request from the UWP app
        /// </summary>
        private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            Console.WriteLine("Connection_RequestReceived");

            foreach (object key in args.Request.Message.Keys)
            {
                if ((string)key == "GetUsbData")
                {
                    int? key1 = args.Request.Message["GetUsbData"] as int?;

                    UsbDevicesLog usbDevicesLog = new UsbDevicesLog();

                    if (key1 != null)
                    {
                        usbDevicesLog.getUSBdevicesLog();

                        // compose the response as ValueSet
                        ValueSet response = new ValueSet();
                        response.Add("UsbData2UWP", UsbDevicesLog.s_USBdevicesLog);

                        // send the response back to the UWP
                        await args.Request.SendResponseAsync(response);
                    }
                }
                else if ((string)key == "KEY1")
                {
                    int? key1 = args.Request.Message["KEY1"] as int?;
                    int? key2 = args.Request.Message["KEY2"] as int?;

                    if (key1 != null && key2 != null)
                    {
                        int ans = (int)key1 + (int)key2;
                        Console.WriteLine((int)key1 + " + " + (int)key2 + " = " + ans);

                        // compose the response as ValueSet
                        ValueSet response = new ValueSet();
                        response.Add("KEY3", ans);

                        // send the response back to the UWP
                        await args.Request.SendResponseAsync(response);
                    }
                }
            }
        }


        /// <summary>
        /// Handles the event when the app service connection is closed
        /// </summary>
        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            //In console, connection to the UWP lost, so we shut down the desktop process
            Console.WriteLine("UWP lost connection! Please restart.");
            Console.ReadLine();
            Environment.Exit(0);
            //In app, connection to the UWP lost, so we shut down the desktop process
            //Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            //{
            //    Application.Current.Shutdown();
            //}));
        }

        public async void Send2UWP(double a, double b)
        {
            // ask the UWP to calculate d1 + d2
            ValueSet request = new ValueSet();
            request.Add("D1", a);
            request.Add("D2", b);

            //start sending
            AppServiceResponse response = await connection.SendMessageAsync(request);
            //get response
            double result = (double)response.Message["RESULT"];

            Console.WriteLine(result.ToString());
        }

        public async void Send2UWP(string a, string b)
        {
            // ask the UWP to calculate d1 + d2
            ValueSet request = new ValueSet();
            request.Add("s_a", a);
            request.Add("s_b", b);

            //start sending
            AppServiceResponse response = await connection.SendMessageAsync(request);
            //get response
            string result = response.Message["toConsole_result"] as string;

            Console.WriteLine("send data_a to UWP: " + a);
            Console.WriteLine("send data_b to UWP: " + b);
            Console.WriteLine("getting data from UWP: " + result);
        }
        public async void Send2UWP(string a)
        {
            // ask the UWP to calculate d1 + d2
            ValueSet request = new ValueSet();
            request.Add("s_a", a);

            //start sending
            AppServiceResponse response = await connection.SendMessageAsync(request);
            //get response
            //string result = response.Message["toConsole_result"] as string;

            //Console.WriteLine("send data_a to UWP: " + a);
            //Console.WriteLine("getting data from UWP: " + result);
        }
    }
}
