using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CollectDataAP
{
    class UsbDevicesLog
    {
        public static string s_USBdevicesLog = "";
        private static string USBKey = null;

        public void getUSBdevicesLog()
        {
            string DeviceIDTemp = null, name, temp = null;
            List<string> VendorID = new List<string>();                  //類似 C++ STL Vector 函數
            List<string> ProductID = new List<string>();
            List<string> Name = new List<string>();

            //textBox1.Text = "";
            s_USBdevicesLog = "";
            string[] ports = SerialPort.GetPortNames();
            try
            {
                #region USBHub
                ManagementObjectSearcher SearcherUSBHub = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_USBHub");           //使用WMI搜尋紀錄 (只搜尋Win32_USBHub表 欄位不限制)
                foreach (ManagementObject QueryObj in SearcherUSBHub.Get())                                                                                                   //Get動作
                {
                    DeviceIDTemp = Convert.ToString(QueryObj["DeviceID"] + "\r\n");                                                                             //先取得他的ID  
                    if (DeviceIDTemp.IndexOf("VID") != -1)                                                                                                                       //如果有VID的話代表是一個裝置
                    {
                        if (VendorID.IndexOf(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("VID") + 3, 5)) == -1)                                          //判斷在VendorID裡有沒有重複過  (C++ STL set 集合)
                        {
                            if (DeviceIDTemp.Substring(0, DeviceIDTemp.IndexOf("\\")).IndexOf("USB") != -1)                                                                 // 找DeviceID有沒有 USB開頭的路徑
                                name = Convert.ToString(QueryObj["Name"]) + "     " + check(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("\\", DeviceIDTemp.IndexOf("\\") + 1) + 1));      //將USB裝置後面的路徑丟進去做判斷  (等於他的 DeviceID)
                            else
                                name = Convert.ToString(QueryObj["Name"]);
                            Name.Add(name);                                                                                                                                           //把資料push進去
                            VendorID.Add(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("VID") + 3, 5));                                                       //把VID後面的數字5碼資料push進去
                            if (ProductID.IndexOf(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("PID") + 3, 5)) == -1)                 //判斷在ProductID裡有沒有重複過  (C++ STL set 功能)
                                ProductID.Add(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("PID") + 3, 5));                                  //把PID後面的數字5碼資料push進去
                        }
                    }
                }
                #endregion

                #region 滑鼠、觸控
                ManagementObjectSearcher SearcherPointingDevice = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PointingDevice");       //使用WMI搜尋紀錄 (只搜尋Win32_PointingDevice表 欄位不限制)
                //滑鼠觸控類紀錄只存在Win32_PointingDevice裡
                foreach (ManagementObject QueryObj in SearcherPointingDevice.Get())
                {
                    DeviceIDTemp = Convert.ToString(QueryObj["PNPDeviceID"] + "\r\n");
                    if (DeviceIDTemp.IndexOf("VID") != -1)
                    {
                        if (VendorID.IndexOf(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("VID") + 3, 5)) == -1)
                        {
                            Name.Add(Convert.ToString(QueryObj["Name"]));
                            VendorID.Add(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("VID") + 3, 5));
                            if (ProductID.IndexOf(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("PID") + 3, 5)) == -1)
                                ProductID.Add(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("PID") + 3, 5));
                        }
                    }
                }
                #endregion

                #region PnPentity
                ManagementObjectSearcher SearcherPnPentity = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPentity");
                //GPS 和 無線網路的相關內容在此表裡面
                foreach (ManagementObject QueryObj in SearcherPnPentity.Get())
                {
                    DeviceIDTemp = Convert.ToString(QueryObj["PNPDeviceID"] + "\r\n");              //找出PNPDeviceID的資料
                    if (DeviceIDTemp.IndexOf("VID") != -1)                                                              //如果找到VID就代表是個裝置
                    {
                        if (VendorID.IndexOf(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("VID") + 3, 5)) == -1)                 //如果當前的VID沒有在VendorID裡面的話 即沒有重複
                        {
                            ManagementObjectSearcher SearcherPnPentityAgain = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPentity");
                            //在Win32_PnPentity資料表內：
                            // SD具有VID 但裝置路徑不等於真正SD的DeviceID 此類SD稱為SD_VID
                            //SD不具有VID但裝置路徑等於真正SD的DevicelD此類SD稱為SD_DeviceID
                            //先搜尋SD_VID取得正確的VID & PID & Name & 再以SD_VID資料都正確為條件，都成立的話
                            //再重新搜尋整個Win32_PnPentity表，找出SD_DeviceID的路徑以便判斷是否為SD Card 

                            name = Convert.ToString(QueryObj["Name"]);             //先儲存 SD_VID_Name
                            if (name.IndexOf("SD") != -1)                                        //如果Win32_PnPentity表內有正確的SD_VID_Name的話 就開始以下動作
                                foreach (ManagementObject queryObj in SearcherPnPentityAgain.Get())                    //從新的Win32_PnPentity表在一次get
                                {
                                    if (Convert.ToString(queryObj["Caption"]).IndexOf("SD") != -1)                  //如果有找到SD開頭資料的話
                                    {
                                        string PNPDeviceIDAgain = Convert.ToString(queryObj["PNPDeviceID"]);                       //儲存他的DeviceID
                                        if (PNPDeviceIDAgain.Substring(0, PNPDeviceIDAgain.IndexOf("\\")).IndexOf("SD") != -1)                //如果他的路徑是以SD為開頭的路徑話
                                        {
                                            name = Convert.ToString(queryObj["Name"]) + "     " + check(PNPDeviceIDAgain.Substring(PNPDeviceIDAgain.IndexOf("\\", PNPDeviceIDAgain.IndexOf("\\") + 1) + 1));            //擷取路徑後半段的DeviceID去做判斷
                                            if (check(PNPDeviceIDAgain.Substring(PNPDeviceIDAgain.IndexOf("\\", PNPDeviceIDAgain.IndexOf("\\") + 1) + 1)) == "SD")
                                                temp = name;
                                        }
                                    }
                                }
                            if (temp != null)
                            {
                                Name.Add(temp);                                                                   //存入名稱 (如果沒觸發SD_DeviceID的話不會串接 --SD )
                                temp = null;
                            }
                            else
                                Name.Add(name);
                            VendorID.Add(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("VID") + 3, 5));               //存入SD_VID的VID

                            if (ProductID.IndexOf(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("PID") + 3, 5)) == -1)
                                ProductID.Add(DeviceIDTemp.Substring(DeviceIDTemp.IndexOf("PID") + 3, 5));
                        }

                    }
                }
                #endregion

                #region RFentity
                int RFcount = 0;
                ManagementObjectSearcher SearcherRFentity = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapter");
                ManagementObjectCollection collection = SearcherRFentity.Get();
                var networkList = from n in collection.Cast<ManagementBaseObject>()
                                  select new
                                  {
                                      name = n.GetPropertyValue("Name"),
                                      PNP = n.GetPropertyValue("pnpdeviceid")
                                  };
                foreach (var n in networkList)
                {
                    DeviceIDTemp = Convert.ToString(n.PNP + "\r\n");              //找出PNPDeviceID的資料
                    if (DeviceIDTemp.IndexOf("PCI") != -1)
                    {
                        RFcount++;
                        //textBox1.Text += "Name ：" + n.name.ToString() + "\r\n\r\n";
                        Console.WriteLine(s_USBdevicesLog += ("Name ：" + n.name.ToString() + "\r\n\r\n"));
                    }
                }
                #endregion

                double timeStamp = DateTime.UtcNow.AddHours(8).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                DateTime gtm = (new DateTime(1970, 1, 1)).AddSeconds(Convert.ToInt32(timeStamp));

                for (int i = 0; i < Name.Count; i++)
                    /*textBox1.Text += "Name ：" + Name[i] + "\r\n" +
                                        "Vendor ID ： " + VendorID[i] + "\r\n" +
                                        "Product ID ： " + ProductID[i] + "\r\n\r\n";*/
                    Console.WriteLine(s_USBdevicesLog += ("Name ：" + Name[i] + "\r\n" +
                                        "Vendor ID ： " + VendorID[i] + "\r\n" +
                                        "Product ID ： " + ProductID[i] + "\r\n\r\n"));


                for (int j = 0; j < ports.Length; j++)
                    //textBox1.Text += ports[j] + "\r\n";
                    Console.WriteLine(s_USBdevicesLog += (ports[j] + "\r\n"));
                /*
                textBox1.Text += gtm + " the RF Devices total ：" + RFcount + "\r\n\r\n";
                textBox1.Text += gtm + " the USB Devices total ：" + Name.Count + "\r\n\r\n";
                textBox1.Text += gtm + " the COM Devices total :  " + ports.Length + "\r\n\r\n";
                textBox1.Text += "--------------------------------------------------------------------\r\n";*/
                Console.WriteLine(s_USBdevicesLog += (gtm + " the RF Devices total ：" + RFcount + "\r\n\r\n"));
                Console.WriteLine(s_USBdevicesLog += (gtm + " the USB Devices total ：" + Name.Count + "\r\n\r\n"));
                Console.WriteLine(s_USBdevicesLog += (gtm + " the COM Devices total :  " + ports.Length + "\r\n\r\n"));
                Console.WriteLine(s_USBdevicesLog += ("--------------------------------------------------------------------\r\n"));
                // Get the directories currently on the C drive.
                //DirectoryInfo[] cDirs = new DirectoryInfo(@"c:\").GetDirectories();

                using (FileStream fs = new FileStream("C:\\Users\\" + Environment.UserName + "\\Desktop\\USBdevicesFullLog.txt", FileMode.Append))
                {
                    // Write each directory name to a file.
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(s_USBdevicesLog);
                        sw.Close();
                    }
                }
                using (FileStream fs = new FileStream("C:\\Users\\" + Environment.UserName + "\\Desktop\\USBdevicesFullLog.txt", FileMode.Append))
                {
                    // Write each directory name to a file.
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(gtm + " the RF Devices total ：" + RFcount);
                        sw.WriteLine(gtm + " the USB Devices total ：" + Name.Count);
                        sw.WriteLine(gtm + " the COM Devices total ：" + ports.Length);
                        sw.WriteLine("-------------------------------------------------------------\r\n");
                        sw.Close();
                    }
                }
            }
            catch (ManagementException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private static string check(string source)
        {
            ManagementObjectSearcher SearcherDiskDrive = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive");           //搜尋Win32_DiskDrive
            try
            {
                foreach (ManagementObject QueryObj in SearcherDiskDrive.Get())                           //GET動作
                {
                    string USB_or_SD_DevID = Convert.ToString(QueryObj["PNPDeviceID"]);                       //從Win32_DiskDrive表裡找PNPDeviceID資料
                    string box;
                    if (USB_or_SD_DevID.Substring(0, USB_or_SD_DevID.IndexOf("\\")).IndexOf("USB") != -1)                   //如果路徑開頭是USB
                    {
                        box = USB_or_SD_DevID.Substring(USB_or_SD_DevID.IndexOf("\\", USB_or_SD_DevID.IndexOf("\\") + 1) + 1);       //從Win32_DiskDrive表裡找PNPDeviceID的路徑 即DeviceID
                        if (source.Substring(0, 7) == box.Substring(0, 7))       //取前7個比對 避免後面會有亂碼或無意義後綴
                        {
                            USBKey += source + ",";                    //串接USBKey
                            return "USB";
                        }

                    }

                    if (USB_or_SD_DevID.Substring(0, USB_or_SD_DevID.IndexOf("\\")).IndexOf("SD") != -1)
                    {
                        box = USB_or_SD_DevID.Substring(USB_or_SD_DevID.IndexOf("\\", USB_or_SD_DevID.IndexOf("\\") + 1) + 1);
                        if (source.Substring(0, 6) == box.Substring(0, 6))
                        {
                            MessageBox.Show(box + " " + source);
                            return "SD";
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
            }
            return null;

        }
        /*
        protected override void WndProc(ref Message m)
        {

            switch (m.Msg)
            {
                case 0x0218: //WM_POWERBROADCAST
                    switch (m.WParam.ToInt32())
                    {
                        case 0x4: //PBT_APMSUSPEND
                            break;
                        //case 0x6: //PBT_APMRESUMECRITICAL
                        case 0x7: //PBT_APMRESUMESUSPEND
                                  // case 0x12: //PBT_APMRESUMEAUTOMATIC
                            button1.PerformClick();     //觸發
                            break;
                    }
                    break;
            }

            base.WndProc(ref m);
        }
        */

        private static bool isNatural(string str)
        {
            System.Text.RegularExpressions.Regex reg1 = new System.Text.RegularExpressions.Regex(@"^[A-Za-z]+$");
            return reg1.IsMatch(str);
        }           // 相當於C++的isalpha函數

        private string disk_Check(int index)
        {
            int check;
            int count = 0;
            ManagementObjectSearcher SearcherLogicalDisk = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_LogicalDisk");           //搜尋Win32_DiskDrive
            try
            {
                foreach (ManagementObject QueryObj in SearcherLogicalDisk.Get())                           //GET動作
                {
                    check = Convert.ToInt32(QueryObj["DriveType"]);
                    if (check == 3)
                        count++;
                }
                count += index;
                ManagementObjectSearcher SearcherLogicalDiskRootDirectory = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_LogicalDiskRootDirectory");
                foreach (ManagementObject QueryObj in SearcherLogicalDiskRootDirectory.Get())
                {
                    string t = Convert.ToString(QueryObj["PartComponent"]);           //累加硬碟的數量
                    if (isNatural(t.Substring(t.IndexOf("Name=") + 6, 1)))
                        if (--count == 0)
                            return t.Substring(t.IndexOf("Name=") + 6, 1);
                }



                //      C_Check(count);
                return "Error";
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
                return "Error";
            }
        }
    }
}
