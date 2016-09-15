using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Int64 offset = Addresses.Base.ToInt64() + address;
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)H5Fpid);
            int unused = 0;
            IntPtr addr = new IntPtr(offset);
            byte[] hex = new byte[4];
            ReadProcessMemory(hProc, addr, hex, (UInt32)hex.LongLength, ref unused);
            return hex;
        }

        public static void WriteToAddress(Int32 address, byte[] hex)
        {
            Int64 offset = Addresses.Base.ToInt64() + address;
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)H5Fpid);
            int unused = 0;
            IntPtr addr = new IntPtr(offset);
            WriteProcessMemory(hProc, addr, hex, (UInt32)hex.LongLength, out unused);

            CloseHandle(hProc);
        }
    }

    class Addresses
    {
        public static IntPtr Base = Process.GetProcessesByName("halo5forge").FirstOrDefault().MainModule.BaseAddress;
        public static string baseAddress = Base.ToString("X"); // Base memory address
        public static Int32 FOV = 0x58ECF90; // FOV memory address
        public static List<Int32> FPS = new List<Int32>(new Int32[] { 0x34B8C50, 0x34B8C60, 0x34B8C70 }); // FPS memory addresses
        //public static Int32 DOF = Int32.Parse(baseAddress) + ; // DOF memory address
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
                while (CommandGet.FOV().Equals(0))
                    Thread.Sleep(200);
                return p;
            }
            return null;
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

    class CommandGet
    {
        public static float FOV()
        {
            return BitConverter.ToSingle(Memory.ReadFromAddress(Addresses.FOV), 0);
        }

        public static int FPS()
        {
            return 1000000 / BitConverter.ToInt16(Memory.ReadFromAddress(Addresses.FPS[0]), 0);
        }

        /*public static int DOF()
        {
            return BitConverter.ToInt16(Memory.ReadFromAddress(Addresses.DOF), 0);
        }*/
    }

    class CommandSet
    {
        public static void FOV(float newFOV = 78)
        {
            if (!newFOV.Equals(78))
                Console.WriteLine("Setting FPS to {0}.", newFOV);
            else
                Console.WriteLine("Setting FOV back to default.");

            switch (newFOV >= 65 && newFOV <= 150)
            {
                case true:
                    Memory.WriteToAddress(Addresses.FOV, BitConverter.GetBytes(newFOV));
                    break;
                case false:
                    if (newFOV < 65)
                        Console.WriteLine("Not possible to set FOV lower than 65."); // Force user to stay higher than or equal 65fov
                    else if (newFOV > 150)
                        Console.WriteLine("Not possible to set FOV higher than 150."); // Force user to stay lower than or equal 150fov
                    break;
            }
        }

        public static void FPS(float newFPS = 60)
        {
            if (!newFPS.Equals(60))
                Console.WriteLine("Setting FPS to {0}.", newFPS);
            else
                Console.WriteLine("Setting FPS back to default.");

            switch (newFPS >= 30 && newFPS <= 300)
            {
                case true:
                    for (int i = 0; i < Addresses.FPS.Count; i++)
                        Memory.WriteToAddress(Addresses.FPS[i], BitConverter.GetBytes(1000000 / Convert.ToInt16(newFPS)));
                    break;
                case false:
                    if (newFPS < 30)
                        Console.WriteLine("Not possible to set FPS lower than 30."); // Force user to stay higher than or equal 30fps
                    else if (newFPS > 300)
                        Console.WriteLine("Not possible to set FPS higher than 300."); // Force user to stay lower than or equal 300fps
                    break;
            }
        }

        /*public static void DOF(float newDOF = )
        {
            Memory.WriteToAddress(Addresses.DOF, BitConverter.GetBytes(newDOF));
        }*/
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
                        return Text[0]; // Tell user FOV help options
                    }
                    else if (grab.EndsWith("fps"))
                    {
                        return Text[1]; // Tell user FPS help options
                    }
                    else if (grab.EndsWith("exit"))
                    {
                        return Text[2]; // Tell user Exit help options
                    }
                    else return "Invalid argument.";
            }
            return "";
        }
    }
}