using System;
using System.Diagnostics;

using Manager;

namespace Twisters_H5F_Console_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading memory...");
        reload:
            if (!UWP.H5Fprocess().Equals(null))
            {
                Console.Clear();
                Other.SetH5FToForeground();
                Other.SetOwnToForeground();
                //Console.WriteLine("Base Address: {0}{1}", Addresses.baseAddress, Environment.NewLine);
                Console.WriteLine("Current FOV: {0}", CommandGet.FOV()); // Tell user current FOV
                Console.WriteLine("Current FPS: {0}", CommandGet.FPS()); // Tell user current FPS
                Console.WriteLine("{0}Enter a Command or Type Help to Show help:", Environment.NewLine); // Grab input from user

                while (true) // Main loop
                {
                    string input = Console.ReadLine(); // Get input from user

                    if (input.ToLower().Equals("reload"))
                        goto reload;
                    else if (input.ToLower().Contains("help"))
                        Console.WriteLine(Help.Get(input.ToLower()));
                    else if (input.ToLower().Equals("exit"))
                        Environment.Exit(0); // Exit the appliction
                    else if (!(input.ToLower().StartsWith("fov") || input.ToLower().StartsWith("fps")))
                        Console.WriteLine("Please enter a valid command.");

                    string[] Input = input.Split(' '); // Split input into array
                    string in1st = Input[0].ToLower();
                    string in2nd = Input[1].ToLower();

                    if (!in2nd.Equals(null))
                    {
                        Int32 n;
                        bool isNumeric = Int32.TryParse(in2nd, out n); // Check array entry 2 for int

                        if (in1st.Equals("fov")) // If array entry 1 lower case equals "fov" continue
                            if (in2nd.Equals("default"))
                                CommandSet.FOV(); // Set FOV to default
                            else if (isNumeric.Equals(true)) // If array entry 2 Has Int Continue
                                CommandSet.FOV(n); // Set FPS to array entry 2
                            else
                                Console.WriteLine("{0} is not a valid argument for FOV, type Help for examples", Input[0]);
                        else if (in1st.Equals("fps")) // If array entry 1 lower case equals "fps" continue
                            if (in2nd.Equals("default"))
                                CommandSet.FPS(); // Set FPS to default
                            else if (isNumeric.Equals(true)) // If array entry 2 has int continue
                                CommandSet.FPS(n); // Set FPS to array entry 2
                            else
                                Console.WriteLine("{0} is not a valid argument for FPS, type Help for examples", Input[1]);
                    }
                }
            }
        }
    }
}
