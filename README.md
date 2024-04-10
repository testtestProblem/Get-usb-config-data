# UI 
![Untitled](https://github.com/testtestProblem/Get-usb-config-data/assets/107662393/32d95733-eadb-4474-b6a6-ea16f138ab32)

# LayoutKind Enum(structure memery layout)
* Usually layout structure  
Explicit	2	  
The precise position of each member of an object in unmanaged memory is explicitly controlled, subject to the setting of the Pack field. Each member must use the FieldOffsetAttribute to indicate the position of that field within the type.   
Sequential	0	  
The members of the object are laid out sequentially, in the order in which they appear when exported to unmanaged memory. The members are laid out according to the packing specified in Pack, and can be noncontiguous.  
* Example code  
```C#
[StructLayout(LayoutKind.Sequential)]
public struct Point
{
   public int x;
   public int y;
}

[StructLayout(LayoutKind.Explicit)]
public struct Rect
{
   [FieldOffset(0)] public int left;
   [FieldOffset(4)] public int top;
   [FieldOffset(8)] public int right;
   [FieldOffset(12)] public int bottom;
}
```
# Create txt file in desktop
```C#
using (FileStream fs = new FileStream("C:\\Users\\" + Environment.UserName + "\\Desktop\\USBdevicesFullLog.txt", FileMode.Append))
                {
                    // Write each directory name to a file.
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(s_USBdevicesLog);
                        sw.WriteLine("--------------------------------------------------------------------\r\n\r\n");
                        sw.Close();
                    }
                }
```
# Detect Modern standby
* Reference - https://blog.csdn.net/mochounv/article/details/114668594
