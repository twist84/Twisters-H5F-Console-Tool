using System;

using Manager;

namespace Twisters_H5F_Console_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            LaunchHalo:
            Console.WriteLine("Loading memory...");
            if (!UWP.H5Fprocess().Equals(null))
                Console.Clear();

            Console.WriteLine("Info: FOV: {0}, FPS: {1}", Commands.Get("FOV"), Commands.Get("FPS"));
            Console.WriteLine("Debug: Process Id: {0}, Base Address: {1}", Memory.H5Fpid, Addresses.baseAddress);
            Console.WriteLine("{0}Enter a Command or Type Help to Show help:", Environment.NewLine);

            while (!UWP.H5FIsRunning().Equals(false))
                Commands.Try(Console.ReadLine().Split(' '));

            Console.WriteLine("Halo 5 Forge is not currently running type LaunchHalo to continue.");
            if (Console.ReadLine().Equals("LaunchHalo"))
                goto LaunchHalo;
        }
    }
}
