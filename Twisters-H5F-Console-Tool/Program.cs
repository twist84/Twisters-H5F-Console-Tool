using System;

using Commands;
using Memory;

using System.Diagnostics;

namespace Twisters_H5F_Console_Tool
{
    class Program
    {
        static void Main(string[] args)
        {
            Process p = default(Process);
            try
            {
                p = Process.GetProcessById((int)Manager.H5Fpid);
            }
            catch { }

            if ((p != null))
            {
                Other.SetOwnToForeground();
                //Console.WriteLine("Base Address: {0}{1}", Addresses.baseAddress, Environment.NewLine);
                try
                {
                    Get.FOV();
                }
                catch { }

                if((Get.FOV() != 0))
                {
                    Console.WriteLine("Current FOV: {0}", Get.FOV()); // Tell user current FOV
                }
                else
                    Console.WriteLine("Current FOV unavailable (game not fully loaded maybe?)");

                Console.WriteLine("Current FPS: {0}", Get.FPS()); // Tell user current FPS
                Console.WriteLine("{0}Enter a Command or Type Help to Show help:", Environment.NewLine);

                while (true) // Main loop
                {
                    string input = Console.ReadLine(); // Get input from user

                    switch (input.ToLower()) // Switch with lower case user input
                    {
                        case "help":
                            Console.WriteLine("Type FOV and Default or a value between 65-150,\nI.E. fov default or fov 110"); // Tell user FOV help options
                            Console.WriteLine("Type FPS and Default or a value between 30-300,\nI.E. fps default or fps 110"); // Tell user FPS help options
                            Console.WriteLine("Type Exit to close the application."); // Tell user Exit help options
                            break;
                        case "exit":
                            Console.WriteLine("Exiting the application.");
                            Environment.Exit(0); // Exit the appliction
                            break;
                        default:
                            if (!(input.ToLower().StartsWith("fov") || input.ToLower().StartsWith("fps")))
                                Console.WriteLine("Please enter a valid command.");
                            break;
                    }

                    string[] Input = input.Split(' '); // Split input into array

                    if (Input.Length.Equals(2)) // If array has 2 entries continue
                    {
                        Int32 n;
                        bool isNumeric = Int32.TryParse(Input[1], out n); // Check array entry 2 for int

                        if (Input[0].ToLower().Equals("fov")) // If array entry 1 lower case equals "fov" continue
                        {
                            if (isNumeric.Equals(true)) // If array entry 2 Has Int Continue
                            {
                                Console.WriteLine("Setting FOV to {0}.", Int32.Parse(Input[1]));
                                Set.FOV(Int32.Parse(Input[1])); // Set FOV to array entry 2
                            }
                            else if (Input[1].ToLower().Equals("default")) // If array entry 2 lower case equals "default" continue
                            {
                                Console.WriteLine("Setting FOV back to default.");
                                Set.FOV(); // Set FOV to default
                            }
                        }
                        else if (Input[0].ToLower().Equals("fps")) // If array entry 1 lower case equals "fps" continue
                        {
                            if (isNumeric.Equals(true)) // If array entry 2 has int continue
                            {
                                Console.WriteLine("Setting FPS to {0}.", Int32.Parse(Input[1]));
                                Set.FPS(Int32.Parse(Input[1])); // Set FPS to array entry 2
                            }
                            else if (Input[1].ToLower().Equals("default")) // If array entry 2 lower case equals "default" continue
                            {
                                Console.WriteLine("Setting FPS back to default.");
                                Set.FPS(); // Set FPS to default
                            }
                        }
                    }
                }
            }
        }
    }
}
