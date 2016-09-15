using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Manager
{
    class Memory // Taken from https://github.com/CorpenEldorito/Corps-H5F-Tool Credit goes to Corpen.
    {
        public static int H5Fpid = (int)UWP.LaunchApp("Microsoft.Halo5Forge_1.114.4592.2_x64__8wekyb3d8bbwe");

        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        public static byte[] ReadFromAddress(Int32 address)
        {
            Int64 offset = Addresses.Base().ToInt64() + address;
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)H5Fpid);
            int unused = 0;
            IntPtr addr = new IntPtr(offset);
            byte[] hex = new byte[4];
            ReadProcessMemory(hProc, addr, hex, (UInt32)hex.LongLength, ref unused);
            return hex;
        }

        public static void WriteToAddress(Int32 address, byte[] hex)
        {
            Int64 offset = Addresses.Base().ToInt64() + address;
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)H5Fpid);
            int unused = 0;
            IntPtr addr = new IntPtr(offset);
            WriteProcessMemory(hProc, addr, hex, (UInt32)hex.LongLength, out unused);

            CloseHandle(hProc);
        }
    }

    class Addresses
    {
        public static IntPtr Base()
        {
            IntPtr p = default(IntPtr);
            try
            {
                p = Process.GetProcessById(Memory.H5Fpid).MainModule.BaseAddress;
            }
            catch { }
            
            return p;
        }
        public static string baseAddress = Base().ToString("X");
        public static Int32 FOV = 0x58ECF90;
        public static List<Int32> FPS = new List<Int32>(new Int32[] { 0x34B8C50, 0x34B8C60, 0x34B8C70 });
        //public static Int32 DOF = Int32.Parse(baseAddress) + ;
    }

    class UWP // Taken from http://blogs.microsoft.co.il/pavely/2015/10/24/launching-windows-store-apps-programmatically/ Credit goes to Pavel.
    {
        enum ActivateOptions
        {
            None = 0x00000000,  // No flags set
            DesignMode = 0x00000001,  // The application is being activated for design mode
            NoErrorUI = 0x00000002,  // Do not show an error dialog if the app fails to activate                                
            NoSplashScreen = 0x00000004,  // Do not show the splash screen when activating the app
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("2e941141-7f97-4756-ba1d-9decde894a3d")]
        interface IApplicationActivationManager
        {
            int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId, [MarshalAs(UnmanagedType.LPWStr)] string arguments,
                ActivateOptions options, out uint processId);
            int ActivateForFile([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId, IntPtr pShelItemArray,
                [MarshalAs(UnmanagedType.LPWStr)] string verb, out uint processId);
            int ActivateForProtocol([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId, IntPtr pShelItemArray,
                [MarshalAs(UnmanagedType.LPWStr)] string verb, out uint processId);
        }

        [ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
        class ApplicationActivationManager { }

        [DllImport("kernel32")]
        static extern int OpenPackageInfoByFullName([MarshalAs(UnmanagedType.LPWStr)] string fullName, uint reserved, out IntPtr packageInfo);

        [DllImport("kernel32")]
        static extern int GetPackageApplicationIds(IntPtr pir, ref int bufferLength, byte[] buffer, out int count);

        [DllImport("kernel32")]
        static extern int ClosePackageInfo(IntPtr pir);

        public static uint LaunchApp(string packageFullName, string arguments = null)
        {
            IntPtr pir = IntPtr.Zero;
            OpenPackageInfoByFullName(packageFullName, 0, out pir);

            int length = 0, count;
            GetPackageApplicationIds(pir, ref length, null, out count);

            var buffer = new byte[length];
            GetPackageApplicationIds(pir, ref length, buffer, out count);

            var appUserModelId = Encoding.Unicode.GetString(buffer, IntPtr.Size * count, length - IntPtr.Size * count);
            var activation = (IApplicationActivationManager)new ApplicationActivationManager();
            uint pid;
            int hr = activation.ActivateApplication(appUserModelId, arguments ?? string.Empty, ActivateOptions.NoErrorUI, out pid);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
            return pid;
        }

        public static Process H5Fprocess()
        {
            Process p = default(Process);
            try
            {
                p = Process.GetProcessById(Memory.H5Fpid);
            }
            catch { }

            if (!p.Equals(null))
            {
                while (float.Parse(Commands.Get("FOV")).Equals(0) && Addresses.Base().Equals(0))
                    Thread.Sleep(200);

                Other.SetH5FToForeground();
                Other.SetOwnToForeground();
                return p;
            }
            return null;
        }

        public static bool H5FIsRunning()
        {
            Process p = default(Process);
            try
            {
                p = Process.GetProcessById(Memory.H5Fpid);
            }
            catch { }

            if (!p.Equals(null))
            {
                return true;
            }
            return false;
        }
    }

    class Other
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void SetOwnToForeground()
        {
            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
            //if (!GetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle).Equals(true))
        }

        public static void SetH5FToForeground()
        {
            SetForegroundWindow(Process.GetProcessById(Memory.H5Fpid).MainWindowHandle);
            //if (!GetForegroundWindow(Process.GetProcessById(Memory.H5Fpid).MainWindowHandle).Equals(true))
        }
    }

    class Commands
    {
        public static void Try(string[] Input)
        {
            switch (Input.Length.Equals(2))
            {
                case true:
                    if (!Input[1].ToLower().Equals(null))
                    {
                        Int32 n;
                        bool isNumeric = Int32.TryParse(Input[1].ToLower(), out n);

                        if (Input[0].ToLower().Equals("fov"))
                            if (Input[1].ToLower().Equals("default"))
                                Commands.Set("FOV", 78);
                            else if (isNumeric.Equals(true))
                                Commands.Set("FOV", n);
                            else
                                Console.WriteLine("{0} is not a valid argument for FOV, type Help for examples", Input[1]);

                        if (Input[0].ToLower().Equals("fps"))
                            if (Input[1].ToLower().Equals("default"))
                                Commands.Set("FPS", 60);
                            else if (isNumeric.Equals(true))
                                Commands.Set("FPS", n);
                            else
                                Console.WriteLine("{0} is not a valid argument for FPS, type Help for examples", Input[1]);
                    }
                    break;
                case false:
                    if (Input[0].ToLower().Contains("help"))
                        Console.WriteLine(Help.Get(Input[0].ToLower()));
                    else if (Input[0].ToLower().Equals("exit"))
                        Environment.Exit(0);
                    else if (Input[0].ToLower().StartsWith("fov") || Input[0].ToLower().StartsWith("fps"))
                        Console.WriteLine("Current FOV: {0}", Commands.Get(Input[0]));
                    else
                        Console.WriteLine("{0} is not a valid command.", Input[0]);
                    break;
            }
        }

        public static string Get(string grab)
        {
            switch (grab.ToUpper())
            {
                case "FOV":
                    return BitConverter.ToSingle(Memory.ReadFromAddress(Addresses.FOV), 0).ToString();
                case "FPS":
                    return (1000000 / BitConverter.ToInt16(Memory.ReadFromAddress(Addresses.FPS[0]), 0)).ToString();
                case "DOF":
                    break;
            }
            return "";
        }

        public static void Set(string grab, float newVal)
        {
            switch (grab)
            {
                #region FOV
                case "FOV":
                    if (!newVal.Equals(78))
                        Console.WriteLine("Setting FPS to {0}.", newVal);
                    else
                        Console.WriteLine("Setting FOV back to default.");

                    switch (newVal >= 65 && newVal <= 150)
                    {
                        case true:
                            Memory.WriteToAddress(Addresses.FOV, BitConverter.GetBytes(newVal));
                            break;
                        case false:
                            if (newVal < 65)
                                Console.WriteLine("Not possible to set FOV lower than 65.");
                            else if (newVal > 150)
                                Console.WriteLine("Not possible to set FOV higher than 150.");
                            break;
                    }
                    break;
                #endregion
                #region FPS
                case "FPS":
                    if (!newVal.Equals(60))
                        Console.WriteLine("Setting FPS to {0}.", newVal);
                    else
                        Console.WriteLine("Setting FPS back to default.");

                    switch (newVal >= 30 && newVal <= 300)
                    {
                        case true:
                            for (int i = 0; i < Addresses.FPS.Count; i++)
                                Memory.WriteToAddress(Addresses.FPS[i], BitConverter.GetBytes(1000000 / Convert.ToInt16(newVal)));
                            break;
                        case false:
                            if (newVal < 30)
                                Console.WriteLine("Not possible to set FPS lower than 30.");
                            else if (newVal > 300)
                                Console.WriteLine("Not possible to set FPS higher than 300.");
                            break;
                    }
                    break;
                #endregion
                #region DOF
                case "DOF":
                    break;
                #endregion
            }
        }
    }

    class Help
    {
        static string[] Text = new string[] 
        {
            "Type FOV and Default or a value between 65-150,\nI.E. fov default or fov 110",
            "Type FPS and Default or a value between 30-300,\nI.E. fps default or fps 144",
            "Type Exit to close the application."
        };

        public static string Get(string grab = "help")
        {
            switch (grab.Equals("help"))
            {
                case true:
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < Text.Length; i++)
                            sb.AppendLine(Text[i]);
                        return sb.ToString();
                    }
                case false:
                    if (grab.EndsWith("fov"))
                    {
                        return Text[0];
                    }
                    else if (grab.EndsWith("fps"))
                    {
                        return Text[1];
                    }
                    else if (grab.EndsWith("exit"))
                    {
                        return Text[2];
                    }
                    else return "Invalid argument.";
            }
            return "";
        }
    }
}