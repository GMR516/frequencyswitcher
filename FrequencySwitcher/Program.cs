using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FrequencySwitcher;

class Program
{
    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern Boolean EnumDisplayDevices(
        [param: MarshalAs(UnmanagedType.LPTStr)]
        string lpDevice,
        [param: MarshalAs(UnmanagedType.U4)]
        int iDevNum,
        [In, Out]
        ref DISPLAY_DEVICE lpDisplayDevice,
        [param: MarshalAs(UnmanagedType.U4)]
        int dwFlags);

    #region Import Low Level Functions
    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern Boolean EnumDisplaySettings(
        [param: MarshalAs(UnmanagedType.LPTStr)]
        string lpszDeviceName,
        [param: MarshalAs(UnmanagedType.U4)]
        int iModeNum,
        [In, Out]
        ref DEVMODE lpDevMode);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmSpecVersion;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmDriverVersion;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmSize;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmDriverExtra;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmFields;

        public POINTL dmPosition;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayOrientation;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayFixedOutput;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmColor;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmDuplex;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmYResolution;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmTTOption;

        [MarshalAs(UnmanagedType.I2)]
        public Int16 dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 dmLogPixels;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmBitsPerPel;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPelsWidth;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPelsHeight;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayFlags;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDisplayFrequency;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmICMMethod;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmICMIntent;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmMediaType;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmDitherType;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmReserved1;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmReserved2;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPanningWidth;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct DISPLAY_DEVICE
    {
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 cb;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public char[] DeviceName;
        //public byte[] DeviceName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] DeviceString;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 StateFlags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] DeviceID;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public char[] DeviceKey;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
        [MarshalAs(UnmanagedType.I4)]
        public int x;
        [MarshalAs(UnmanagedType.I4)]
        public int y;
    }
    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.I4)]
    public static extern int ChangeDisplaySettings(
        [In, Out]
        ref DEVMODE lpDevMode,
        [param: MarshalAs(UnmanagedType.U4)]
        uint dwflags);

    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.I4)]
    public static extern int ChangeDisplaySettingsEx(
        [param: MarshalAs(UnmanagedType.LPStr)]
        string lpszDeviceName,
        [In, Out]
        ref DEVMODE lpDevMode,
        IntPtr hwnd,
        [param: MarshalAs(UnmanagedType.U4)]
        uint dwflags,
        //TODO: MIGHT BE WRONG?  https://bit.ly/2KAZ6WK
        byte[] lParam);

    #endregion
    static readonly bool isRunningOnBattery = SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline;
    public static int errorStatus;
    private static int bestSetting;
    public static List<string> monitorList;
    /*public static void GetCurrentSettings()
    {
        DEVMODE mode = new DEVMODE();
        mode.dmSize = (ushort)Marshal.SizeOf(mode);

        if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref mode)) // Succeeded
        {
            Console.WriteLine("Current Mode:\n\t" +
                              "{0} by {1}, " +
                              "{2} bit, " +
                              "{3} degrees, " +
                              "{4} hertz",
                mode.dmPelsWidth,
                mode.dmPelsHeight,
                mode.dmBitsPerPel,
                mode.dmDisplayOrientation * 90,
                mode.dmDisplayFrequency);
        }
    }*/

    public static void CalculateSupportedModes()
    {
        DEVMODE mode = new DEVMODE();
        mode.dmSize = (ushort)Marshal.SizeOf(mode);
        DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
        displayDevice.cb = (ushort)Marshal.SizeOf(displayDevice);

        int modeIndex = 0;
        monitorList = new List<string>();

        int iDevNum = 0;
        while (EnumDisplayDevices(null, iDevNum, ref displayDevice, 0))
        {
            string deviceNameBuilder = displayDevice.DeviceName.Aggregate("", (current, c) => current + c);
            monitorList.Add(deviceNameBuilder.Trim());
            iDevNum++;
        }
        while (EnumDisplaySettings(null, modeIndex, ref mode))
        {
            bestSetting = modeIndex;
            modeIndex++;
        }
    }

    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            if (args[0].Trim().ToLower() == "-batterycheck")
            {
                CalculateSupportedModes();
                DEVMODE originalMode = new DEVMODE();
                originalMode.dmSize = (ushort)Marshal.SizeOf(originalMode);
                //TODO: Figure out how to get the current setting.
                EnumDisplaySettings(null, bestSetting, ref originalMode);
                DEVMODE newMode = originalMode;

                newMode.dmDisplayFrequency = isRunningOnBattery ? (uint)60 : 144;

                ChangeDisplaySettings(ref newMode, 0);
            }
            else
            {

                CalculateSupportedModes();
                DEVMODE originalMode = new DEVMODE();
                originalMode.dmSize = (ushort)Marshal.SizeOf(originalMode);
                //TODO: Figure out how to get the current setting.
                EnumDisplaySettings(null, bestSetting, ref originalMode);
                DEVMODE newMode = originalMode;

                //Width
                uint w = newMode.dmPelsWidth;
                //Height
                uint h = newMode.dmPelsHeight;
                //Frequency
                uint f = newMode.dmDisplayFrequency;
                //Display (if specific)
                int d = -1;
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].Trim().ToLower() == "-w" && args[i + 1] != null)
                        w = uint.Parse(args[i + 1]);
                    if (args[i].Trim().ToLower() == "-h" && args[i + 1] != null)
                        h = uint.Parse(args[i + 1]);
                    if (args[i].Trim().ToLower() == "-f" && args[i + 1] != null)
                        f = uint.Parse(args[i + 1]);
                    if (args[i].Trim().ToLower() == "-d" && args[i + 1] != null)
                        d = int.Parse(args[i + 1]);
                }
                newMode.dmPelsWidth = w;
                newMode.dmPelsHeight = h;
                newMode.dmDisplayFrequency = f;

                //If no specific monitor was specified.
                if (d == -1)
                {
                    foreach (string s in monitorList)
                    {
                        int status = ChangeDisplaySettingsEx(s, ref newMode, IntPtr.Zero, 0, null);
                        //TODO: Monitors that don't exist throw the same error code as monitors that do but someone typed something wrong.
                        //https://i.imgur.com/lcGWmvZ.png
                        //https://i.imgur.com/RO4Tn2R.png
                        /*if (status != 0)
                        {
                            errorStatus = 1;
                            ShowError();
                        }*/
                    }
                }
                else
                {
                    //TODO: Same as above.
                    int status = ChangeDisplaySettingsEx($@"\\.\DISPLAY" + d, ref newMode, IntPtr.Zero, 0, null);
                }
            }
        }
        else
        {
            ShowError();
        }
    }

    static void ShowError()
    {
        Form1 form = new Form1();
        form.ShowDialog();
    }
}