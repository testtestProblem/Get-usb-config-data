using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HotTab_Win10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int rfDevice = 0, usbDevice = 0, comDevice = 0;
        int rfDevice2 = 0, usbDevice2 = 0, comDevice2 = 0;
        public MainPage()
        {
            this.InitializeComponent();

            // ApplicationView.PreferredLaunchViewSize = new Size(200, 200);
            // ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                //for connect or disconnect event
                App.AppServiceConnected += MainPage_AppServiceConnected;
                App.AppServiceDisconnected += MainPage_AppServiceDisconnected;

                //ApplicationData.Current.LocalSettings.Values["parameters"] = "test";

                //for sideload app
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }

            //getUsbData();
        }


        /// <summary>
        /// When the desktop process is connected, get ready to send/receive requests
        /// </summary>
        private async void MainPage_AppServiceConnected(object sender, AppServiceTriggerDetails e)
        {
            App.Connection.RequestReceived += AppServiceConnection_RequestReceived;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // enable UI to access  the connection
                //btnRegKey.IsEnabled = true;
            });
        }

        /// <summary>
        /// Handle calculation request from desktop process
        /// (dummy scenario to show that connection is bi-directional)
        /// </summary>
        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            //get value from sideload app
            string s_a = (string)args.Request.Message["s_a"];
            //string s_b = (string)args.Request.Message["s_b"];
            //double result = d1 + d2;
            //data_textBlock.Text = s_a;
            /*
            //send "have get data" to sideload app
            ValueSet response = new ValueSet();
            response.Add("toConsole_result", "Hello! sideload app");
            await args.Request.SendResponseAsync(response);
            */
            //textBox.Text = ""; //it will error
            //log the getting value in the UI for demo purposes
            /*
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>     //I don't know this code
            {
                textBox.Text += string.Format("Request(getting data from sideload app):\ndata 1: {0} \ndata 2: {1}", s_a, s_b);
            });*/
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>     //I don't know this code
            {
                string[] data = s_a.Split("\r\n\r\n");

                PnPmanager_textBlock.Text = data[0];
                USB_textBlock.Text = data[1];
                ComPorts_textBlock.Text = data[2];
                TotalCount_textBlock.Text = data[3];
                
                string[] countDevice = data[3].Split("  ");

                rfDevice = Int32.Parse(countDevice[1]);
                usbDevice = Int32.Parse(countDevice[3]);
                comDevice = Int32.Parse(countDevice[5]);
            });
        }


        /// <summary>
        /// When the desktop process is disconnected, reconnect if needed
        /// </summary>
        private async void MainPage_AppServiceDisconnected(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Reconnect();
            });
        }

        /// <summary>
        /// Ask user if they want to reconnect to the desktop process
        /// </summary>
        private async void Reconnect()
        {
            if (App.IsForeground)
            {
                MessageDialog dlg = new MessageDialog("Connection to desktop process lost. Reconnect?");
                UICommand yesCommand = new UICommand("Yes", async (r) =>
                {
                    await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                });
                dlg.Commands.Add(yesCommand);
                UICommand noCommand = new UICommand("No", (r) => { });
                dlg.Commands.Add(noCommand);
                await dlg.ShowAsync();
            }
        }

        private async void calc_btn_Click(object sender, RoutedEventArgs e)
        {
            //send value to sideloade app
            ValueSet request = new ValueSet();
            request.Add("KEY1", int.Parse(a_textBlock.Text));
            request.Add("KEY2", int.Parse(b_textBlock.Text));
            AppServiceResponse response = await App.Connection.SendMessageAsync(request);   //send data and get response 

            //display the response key/value pairs
            //tbResult.Text = "";
            foreach (string key in response.Message.Keys)
            {
                ans_textBox.Text = "sended by sideload app\nkey: " + key + "\nvalue: " + response.Message[key];
            }
        }
        public async void getUsbData()
        {
            //send value to sideloade app
            ValueSet request = new ValueSet();
            request.Add("GetUsbData", (int)10);
            AppServiceResponse response = await App.Connection.SendMessageAsync(request);   //send data and get response 

            //display the response key/value pairs
            //tbResult.Text = "";
            foreach (string key in response.Message.Keys)
            {
                //data_textBlock.Text = (string)response.Message[key];
                string responseData = (string)response.Message[key];
                string[] data = responseData.Split("\r\n\r\n");
                
                string[] countDevice = data[3].Split("  ");
                
                int errorFlag = 0;

                rfDevice2 = Int32.Parse(countDevice[1]);
                usbDevice2 = Int32.Parse(countDevice[3]);
                comDevice2 = Int32.Parse(countDevice[5]);

                if (rf_checkBox.IsChecked == true)
                {
                    PnPmanager_textBlock.Text = data[0];

                    if (rfDevice != rfDevice2)
                    {
                        errorLog_textBox1.Text += data[0];
                        errorFlag = 1;
                    }
                }
                else PnPmanager_textBlock.Text = "";

                if (usb_checkBox.IsChecked == true)
                {
                    USB_textBlock.Text = data[1];
                    
                    if (usbDevice != usbDevice2)
                    {
                        errorLog_textBox1.Text += data[1];
                        errorFlag = 1;
                    }
                }
                else USB_textBlock.Text = "";

                if (com_checkBox.IsChecked == true)
                {
                    ComPorts_textBlock.Text = data[2];
                    
                    if (comDevice != comDevice2)
                    {
                        errorLog_textBox1.Text += data[2];
                        errorFlag = 1;
                    }
                }
                else ComPorts_textBlock.Text = "";

                TotalCount_textBlock.Text = data[3];

                if(errorFlag==1) errorLog_textBox1.Text += data[3];
            }
        }
        private void refresh_btn_Click(object sender, RoutedEventArgs e)
        {
            getUsbData();
        }
    }
}
