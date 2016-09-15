using System;
using System.Diagnostics;

using Manager;

namespace Twisters_H5F_Console_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            reload:
            if (UWP.H5Fprocess() != null)
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

                    if (Input.Length.Equals(2)) // If array has 2 entries continue
                    {
                        Int32 n;
                        bool isNumeric = Int32.TryParse(Input[1], out n); // Check array entry 2 for int

                        if (Input[0].ToLower().Equals("fov")) // If array entry 1 lower case equals "fov" continue
                        {
                            Int32 fps = n;
                            if (Input[1].ToLower().Equals("default").Equals(true))
                                CommandSet.FOV(); // Set FOV to default
                            else
                                Console.WriteLine("{0} is not a valid argument for FOV, type Help for examples", Input[1]);

                            if (isNumeric.Equals(true)) // If array entry 2 Has Int Continue
                                if (fps >= 65 && fps <= 150)
                                    CommandSet.FOV(Int32.Parse(Input[1])); // Set FOV to array entry 2
                                else if (fps < 150)
                                    Console.WriteLine("Not possible to set FOV higher than 150."); // Force user to stay lower than or equal 150fov
                                else if (fps > 65)
                                    Console.WriteLine("Not possible to set FOV lower than 65."); // Force user to stay higher than or equal 65fov
                        }
                        else if (Input[0].ToLower().Equals("fps")) // If array entry 1 lower case equals "fps" continue
                        {
                            Int32 fov = n;
                            if (Input[1].ToLower().Equals("default").Equals(true))
                                CommandSet.FPS(); // Set FPS to default
                            else
                                Console.WriteLine("{0} is not a valid argument for FPS, type Help for examples", Input[1]);

                            if (isNumeric.Equals(true)) // If array entry 2 has int continue
                                if (fov >= 30 && fov <= 300)
                                    CommandSet.FPS(Int32.Parse(Input[1])); // Set FPS to array entry 2
                                else if (fov < 300)
                                    Console.WriteLine("Not possible to set FPS higher than 300."); // Force user to stay lower than or equal 300fps
                                else if (fov > 30)
                                    Console.WriteLine("Not possible to set FPS lower than 30."); // Force user to stay higher than or equal 30fps
                        }
                    }
                }
            }
        }
    }
}
