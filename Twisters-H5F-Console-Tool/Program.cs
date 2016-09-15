using System;

using Manager;

namespace Twisters_H5F_Console_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading memory...");
            if (!UWP.H5Fprocess().Equals(null))
            {
                Console.WriteLine("Info: FOV: {0}, FPS: {1}\n", Commands.Get("FOV"), Commands.Get("FPS"));
                //Console.WriteLine("Debug: Base Address: {0}", Addresses.baseAddress);

                Console.WriteLine("Enter a Command or Type Help to Show help:");
            }

            while (!UWP.H5Fprocess().Equals(null))
                Commands.Try(Console.ReadLine().Split(' '));
        }
    }
}
