using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Manager
{
    class Memory // Taken from https://github.com/CorpenEldorito/Corps-H5F-Tool Credit goes to Corpen.
    {
        public static int H5Fpid = (int)UWP.LaunchApp(UWP.GetH5FAppName());

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
        static extern IntPtr OpenProcess(
            ProcessAccessFlags dwDesiredAccess, 
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, 
            int dwProcessId
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(
            IntPtr hProcess, 
            IntPtr lpBaseAddress, 
            byte[] lpBuffer, 
            uint nSize, 
            out int lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess, 
            IntPtr lpBaseAddress, 
            byte[] lpBuffer, 
            uint dwSize, 
            ref int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        public static byte[] ReadFromAddress(Int64 BaseAddress, Int32 Address)
        {
            Int64 offset = BaseAddress + Address;
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)H5Fpid);
            int unused = 0;
            IntPtr addr = new IntPtr(offset);
            byte[] hex = new byte[4];
            ReadProcessMemory(hProc, addr, hex, (UInt32)hex.LongLength, ref unused);
            return hex;
        }

        public static void WriteToAddress(Int64 BaseAddress, Int32 Address, byte[] hex)
        {
            Int64 offset = BaseAddress + Address;
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)H5Fpid);
            int unused = 0;
            IntPtr addr = new IntPtr(offset);
            WriteProcessMemory(hProc, addr, hex, (UInt32)hex.LongLength, out unused);

            CloseHandle(hProc);
        }
    }

    class Addresses
    {
        public static IntPtr getBaseAddress()
        {
            IntPtr p = default(IntPtr);
            try
            {
                p = Process.GetProcessById(Memory.H5Fpid).MainModule.BaseAddress;
            }
            catch { }
            
            return p;
        }
        public static IntPtr getTebBaseAddress()
        {
            IntPtr p = default(IntPtr);
            try
            {
                p = (IntPtr)Tls.GetMainHaloThreadInfo().TebBaseAddress;
            }
            catch { }

            return p;
        }
        public static IntPtr BaseAddress = getBaseAddress();
        public static IntPtr TebBaseAddress = getTebBaseAddress();
        public static Int32 FOV = 0x58ECF90;
        public static List<Int32> FPS = new List<Int32>(new Int32[] {
            0x34B8C50,
            0x34B8C60,
            0x34B8C70
        });
        public static List<Int64> DOF = new List<Int64>(new Int64[] {
            0x49B0,
            0x1B7E39C
        });
        public static List<Int32> ResWidth = new List<Int32>(new Int32[] {
            0x4E97F74,
            0x4E97F7C,
            0x4E97F90
            //halo5forge.g_LEngineDefaultPoolId+6B4954
        });
        public static List<Int32> ResHeight = new List<Int32>(new Int32[] {
            0x4E97F78,
            0x4E97F80,
            0x4E97F94
            //halo5forge.g_LEngineDefaultPoolId+6B4958
        });
    }

    class UWP // Taken from http://blogs.microsoft.co.il/pavely/2015/10/24/launching-windows-store-apps-programmatically Credit goes to Pavel.
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

            try
            {
                if (!p.Equals(null))
                {
                    while (Addresses.getBaseAddress().Equals(0))
                        Thread.Sleep(100);
                    while (float.Parse(Commands.Get("FOV")).Equals(0))
                        Thread.Sleep(100);

                    Other.SetH5FToForeground();
                    Other.SetOwnToForeground();
                    return p;
                }
            }
            catch { }
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
            try
            {
                if (!p.Equals(null))
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        public static string GetH5FAppName()
        {
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("Get-AppxPackage");
            foreach (PSObject result in ps.Invoke())
            {
                Regex regex = new Regex(@"Microsoft.Halo5Forge_.*_x64__8wekyb3d8bbwe");
                Match match = regex.Match(result.ToString());
                if (match.Success)
                    return match.Value;
            }
            return null;
        }
    }

    class Other
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void SetOwnToForeground()
        {
            uint ProcessId;
            GetWindowThreadProcessId(GetForegroundWindow(), out ProcessId);

            if (!ProcessId.Equals(Process.GetCurrentProcess().Id))
                SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
        }

        public static void SetH5FToForeground()
        {
            uint ProcessId;
            GetWindowThreadProcessId(GetForegroundWindow(), out ProcessId);

            if (!ProcessId.Equals(Memory.H5Fpid))
                SetForegroundWindow(Process.GetProcessById(Memory.H5Fpid).MainWindowHandle);
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
                        Single n;
                        bool isNumeric = Single.TryParse(Input[1].ToLower(), out n);

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
                    else
                    {
                        Single n,o;
                        bool isNumericW = Single.TryParse(Input[1].ToLower().Split('x')[0], out n);
                        bool isNumericH = Single.TryParse(Input[1].ToLower().Split('x')[0], out o);

                        if (Input[0].ToLower().Equals("rw"))
                            if (Input[1].ToLower().Equals("default"))
                                Commands.Set("RES", 1920, 1080);
                            else if ((isNumericW && isNumericH).Equals(true))
                                Commands.Set("RES", n, o);
                            else
                                Console.WriteLine("{0} is not a valid argument for FOV, type Help for examples", Input[1]);
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
                    return String.Format(
                        "{0:0.00}", 
                        BitConverter.ToSingle(Memory.ReadFromAddress(Addresses.BaseAddress.ToInt64(), Addresses.FOV), 0)
                    );
                case "FPS":
                    return String.Format(
                        "{0:0.00}", 
                        (1000000 / BitConverter.ToInt16(Memory.ReadFromAddress(Addresses.BaseAddress.ToInt64(), Addresses.FPS[0]), 0))
                    );
                case "DOF":
                    break;
                case "RES":
                    return String.Format(
                        "{0}x{1}", 
                        BitConverter.ToInt16(Memory.ReadFromAddress(Addresses.BaseAddress.ToInt64(), Addresses.ResWidth[0]), 0), 
                        BitConverter.ToInt16(Memory.ReadFromAddress(Addresses.BaseAddress.ToInt64(), Addresses.ResHeight[0]), 0)
                    );
                case "AR":
                    break;
            }
            return "";
        }

        public static void Set(string grab, float newVal, float temp = 0)
        {
            switch (grab.ToUpper())
            {
                #region Field Of View
                case "FOV":
                    if (!newVal.Equals(78))
                        Console.WriteLine("Setting FPS to {0}.", newVal);
                    else
                        Console.WriteLine("Setting FOV back to default.");

                    switch (newVal >= 65 && newVal <= 150)
                    {
                        case true:
                            Memory.WriteToAddress(Addresses.BaseAddress.ToInt64(), Addresses.FOV, BitConverter.GetBytes(newVal));
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
                #region Frames Per Second
                case "FPS":
                    if (!newVal.Equals(60))
                        Console.WriteLine("Setting FPS to {0}.", newVal);
                    else
                        Console.WriteLine("Setting FPS back to default.");

                    switch (newVal >= 30 && newVal <= 300)
                    {
                        case true:
                            for (int i = 0; i < Addresses.FPS.Count; i++)
                                Memory.WriteToAddress(Addresses.BaseAddress.ToInt64(), Addresses.FPS[i], BitConverter.GetBytes(1000000 / Convert.ToInt16(newVal)));
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
                #region Depth Of Field
                case "DOF":
                    break;
                #endregion
                #region Resolution
                /*case "RES":
                    if (!(newVal.Equals(960) && temp.Equals(540)).Equals(true))
                        Console.WriteLine("Setting RES to {0}x{1}.", newVal, temp);
                    else
                        Console.WriteLine("Setting RES back to default.");

                    switch (newVal >= 960 && temp >= 540)
                    {
                        case true:
                                Memory.WriteToAddress(Int64.Parse(Addresses.BaseAddress), Addresses.ResWidth[3], BitConverter.GetBytes(Convert.ToInt16(newVal)));
                                Memory.WriteToAddress(Int64.Parse(Addresses.BaseAddress), Addresses.ResHeight[3], BitConverter.GetBytes(Convert.ToInt16(temp)));
                            break;
                        case false:
                            if (newVal < 960 || temp < 540)
                                Console.WriteLine("Not possible to set RES lower than 960x540.");
                            break;
                    }
                    break;*/
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

    unsafe class Tls // Taken from https://github.com/AnvilOnline/AusarDocs Credit goes to xbox7887.
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF
        }

        [Flags]
        public enum ThreadAccessFlags : uint
        {
            All = 0x1F03FF
        }

        public enum ThreadInfoClass : int
        {
            ThreadBasicInformation = 0
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 48)]
        public class ThreadBasicInformation
        {
            public uint ExitStatus;
            public uint _padding;
            public ulong TebBaseAddress;
            public ulong ProcessID;
            public ulong ThreadId;
            public ulong AffinityMask;
            public uint Priority;
            public uint BasePriority;
        }

        [DllImport("kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
        private static extern IntPtr UnmanagedOpenProcess(
            ProcessAccessFlags dwDesiredAccess,
            bool bInheritHandle,
            uint dwProcessId
        );

        [DllImport("kernel32.dll", EntryPoint = "OpenThread", SetLastError = true)]
        private static extern IntPtr UnmanagedOpenThread(
            ThreadAccessFlags dwDesiredAccess,
            bool bInheritHandle,
            uint dwThreadId
        );

        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnmanagedCloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory", SetLastError = true)]
        private static extern bool UnmanagedReadProcessMemory(
            IntPtr hProcess,
            UIntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("ntdll.dll", EntryPoint = "NtQueryInformationThread", SetLastError = true)]
        private static extern int UnmanagedNtQueryInformationThread(
            IntPtr threadHandle,
            ThreadInfoClass threadInformationClass,
            IntPtr threadInformation,
            int threadInformationLength,
            IntPtr returnLengthPtr
        );

        public static IntPtr OpenProcess(ProcessAccessFlags access, bool inheritHandle, uint processId)
        {
            IntPtr handle = UnmanagedOpenProcess(access, inheritHandle, processId);
            if (handle == null)
            {
                throw new Win32Exception();
            }
            return handle;
        }

        public static IntPtr OpenThread(ThreadAccessFlags access, bool inheritHandle, uint threadId)
        {
            IntPtr handle = UnmanagedOpenThread(access, inheritHandle, threadId);
            if (handle == null)
            {
                throw new Win32Exception();
            }
            return handle;
        }

        public static void CloseHandle(IntPtr handle)
        {
            if (!UnmanagedCloseHandle(handle))
            {
                throw new Win32Exception();
            }
        }

        public static void ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead)
        {
            if (!UnmanagedReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, dwSize, out lpNumberOfBytesRead))
            {
                throw new Win32Exception();
            }
        }

        public static ThreadBasicInformation GetThreadInformation(IntPtr threadHandle)
        {
            ThreadBasicInformation info = new ThreadBasicInformation();
            int size = Marshal.SizeOf(typeof(ThreadBasicInformation));
            var buf = Marshal.AllocHGlobal(size);

            if (UnmanagedNtQueryInformationThread(threadHandle, ThreadInfoClass.ThreadBasicInformation, buf, size, IntPtr.Zero) != 0)
            {
                throw new Win32Exception();
            }

            Marshal.PtrToStructure(buf, info);
            Marshal.FreeHGlobal(buf);
            buf = IntPtr.Zero;

            return info;
        }

        public static ThreadBasicInformation GetMainHaloThreadInfo()
        {
            ThreadBasicInformation info = null;

            var process = Process.GetProcessById(Memory.H5Fpid);

            foreach (ProcessThread thread in process.Threads)
            {
                if (thread.ThreadState != System.Diagnostics.ThreadState.Running)
                    continue;

                var threadHandle = OpenThread(ThreadAccessFlags.All, false, (uint)thread.Id);
                info = GetThreadInformation(threadHandle);
                CloseHandle(threadHandle);

                break;
            }
            return info;
        }
    }
}