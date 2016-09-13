/* Taken form https://github.com/CorpenEldorito/Corps-H5F-Tool
 * Credit goes to Corpen
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Memory
{
    class Manager
    {
        public static Process process = Process.GetProcessesByName("halo5forge").FirstOrDefault();

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
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)process.Id);
            int unused = 0;
            IntPtr addr = new IntPtr(offset);
            byte[] hex = new byte[4];
            ReadProcessMemory(hProc, addr, hex, (UInt32)hex.LongLength, ref unused);
            return hex;
        }

        public static void WriteToAddress(Int32 address, byte[] hex)
        {
            Int64 offset = Addresses.Base.ToInt64() + address;
            var hProc = OpenProcess(ProcessAccessFlags.All, false, (int)process.Id);
            int unused = 0;
            IntPtr addr = new IntPtr(offset);
            WriteProcessMemory(hProc, addr, hex, (UInt32)hex.LongLength, out unused);

            CloseHandle(hProc);
        }
    }

    class Addresses
    {
        public static IntPtr Base = Manager.process.MainModule.BaseAddress;
        public static string baseAddress = Base.ToString("X"); // Base memory address
        public static Int32 FOV = 0x58ECF90; // FOV memory address
        public static List<Int32> FPS = new List<Int32>(new Int32[] { 0x34B8C50, 0x34B8C60, 0x34B8C70 }); // FPS memory addresses
        //public static Int32 DOF = Int32.Parse(baseAddress) + ; // DOF memory address
    }
}