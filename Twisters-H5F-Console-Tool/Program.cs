/*using System;
using System.Diagnostics;
using System.IO;

using Manager;
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Twisters_H5F_Console_Tool
{
    class Program
    {
        private static int GetMemoryAddressOfString(byte[] searchedBytes, Process p)
        {
            List<int> addrList = new List<int>();
            int addr = 0;
            int speed = 1024 * 64;
            for (int j = 0x400000; j < 0x7FFFFFFF; j += speed)
            {
                ManagedWinapi.ProcessMemoryChunk mem = new ProcessMemoryChunk(p, (IntPtr)j, speed + searchedBytes.Length);

                byte[] bigMem = mem.Read();

                for (int k = 0; k < bigMem.Length - searchedBytes.Length; k++)
                {
                    bool found = true;
                    for (int l = 0; l < searchedBytes.Length; l++)
                    {
                        if (bigMem[k + l] != searchedBytes[l])
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                    {
                        addr = k + j;
                        break;
                    }
                }
                if (addr != 0)
                {
                    addrList.Add(addr);
                    //addr = 0;
                    break;
                }
            }
            return addrList;
            //return addr;
        }
        /*[DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        static void Main(string[] args)
        {
            Process[] procs = Process.GetProcessesByName("explorer");
            if (procs.Length <= 0)  //proces not found
                return; //can replace with exit nag(message)+exit;
            IntPtr p = OpenProcess(0x10 | 0x20, true, procs[0].Id); //0x10-read 0x20-write

            uint PTR = 0x0; //begin of memory
            byte[] bit2search1 = { 0xEB, 0x20, 0x68, 0x21, 0x27, 0x65 }; //your bit array until ??
            int k = 1;  //numer of missing array (??)
            byte[] bit2search2 = { 0x21, 0x64, 0xA1 };//your bit array after ??
            byte[] buff = new byte[bit2search1.Length + 1 + bit2search2.Length];    //your array lenght;
            int bytesRead;
            bool found = false;
            
            while (PTR != 0xFF000000)   //end of memory // u can specify to read less if u know he does not fill it all
            {
                ReadProcessMemory(p, (IntPtr)PTR, buff, buff.Length, out bytesRead);
                if (SpecialByteCompare(buff, bit2search1, bit2search2, k))
                {
                    //do your stuff
                    found = true;
                    break;
                }
                PTR += 0x1;
            }
            if (!found)
                Console.WriteLine("sorry no byte array found");
        }

        private static bool SpecialByteCompare(byte[] b1, byte[] b2, byte[] b3, int k)  //read memory, first byte array, second byte array, number of missing byte's
        {
            if (b1.Length != (b2.Length + k + b3.Length))
                return false;
            for (int i = 0; i < b2.Length; i++)
            {
                if (b1[i] != b2[i])
                    return false;
            }

            for (int i = 0; i < b3.Length; i++)
            {
                if (b1[b2.Length + k + i] != b3[i])
                    return false;
            }
            return true;
        }

        /*static void Main(string[] args)
        {
            /*Int32 
                InResWidth, InResHeight,
                OutResWidth, OutResHeight;

            InResWidth = BitConverter.ToInt32(Memory.ReadFromAddress(Addresses.BaseAddress.ToInt64(), 0x4E97F60), 0);
            InResHeight = BitConverter.ToInt32(Memory.ReadFromAddress(Addresses.BaseAddress.ToInt64(), 0x4E97F64), 0);

            OutResWidth = BitConverter.ToInt32(Memory.ReadFromAddress(Addresses.BaseAddress.ToInt64(), 0x4E97F90), 0);
            OutResHeight = BitConverter.ToInt32(Memory.ReadFromAddress(Addresses.BaseAddress.ToInt64(), 0x4E97F94), 0);

            Console.WriteLine(
                "Internal Resolution: {0}x{1} Output Resolution: {2}x{3}", 
                InResWidth, InResHeight,
                OutResWidth, OutResHeight
            );
            Console.Read();

            /*LaunchHalo:
            Console.WriteLine("Loading memory...");
            if (!UWP.H5Fprocess().Equals(null))
                Console.Clear();

            Console.WriteLine("Info: FOV: {0}, FPS: {1}, RES: {2}", //, KVD: {3}", 
                Commands.Get("FOV"), Commands.Get("FPS"), Commands.Get("RES")//, Commands.Get("KVD")
            );
            Console.WriteLine("Debug: Process Id: {0}, Base Address: {1}, Teb Base Address: {2}", 
                Memory.H5Fpid, Addresses.BaseAddress.ToString("X"), Addresses.TebBaseAddress);
            Console.WriteLine("{0}Enter a Command or Type Help to Show help:", Environment.NewLine);

            while (!UWP.H5FIsRunning().Equals(false))
                Commands.Try(Console.ReadLine().Split(' '));

            Console.WriteLine("Halo 5 Forge is not currently running type LaunchHalo to continue.");
            if (Console.ReadLine().Equals("LaunchHalo"))
                goto LaunchHalo;
        }*/
    }
}