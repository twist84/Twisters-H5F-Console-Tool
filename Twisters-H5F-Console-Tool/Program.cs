using System;

using Manager;

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
                p = Process.GetProcessById((int)Memory.H5Fpid);
            }
            catch { }

            if ((p != null))
            {
                Other.SetOwnToForeground();
                //Console.WriteLine("Base Address: {0}{1}", Addresses.baseAddress, Environment.NewLine);
                try
                {
                    CommandGet.FOV();
                }
                catch { }

                if(CommandGet.FOV() != 0)
                    Console.WriteLine("Current FOV: {0}", CommandGet.FOV()); // Tell user current FOV
                else
                    Console.WriteLine("Current FOV unavailable (game not fully loaded maybe?)");

                Console.WriteLine("Current FPS: {0}", CommandGet.FPS()); // Tell user current FPS
                Console.WriteLine("{0}Enter a Command or Type Help to Show help:", Environment.NewLine);

                while (true) // Main loop
                {
                    string input = Console.ReadLine(); // Get input from user

                    if (input.ToLower().Contains("help"))
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
                            switch (Input[1].ToLower().Equals("default"))
                            {
                                case true:
                                    Console.WriteLine("Setting FOV back to default.");
                                    CommandSet.FOV(); // Set FOV to default
                                    break;
                                case false:
                                    Console.WriteLine("{0} is not a valid argument for FOV, type Help for examples", Input[1]);
                                    break;
                            }
                            if (isNumeric.Equals(true)) // If array entry 2 Has Int Continue
                            {
                                if (n >= 65 && n <= 150)
                                {
                                    Console.WriteLine("Setting FOV to {0}.", Int32.Parse(Input[1]));
                                    CommandSet.FOV(Int32.Parse(Input[1])); // Set FOV to array entry 2
                                }
                                else if (n < 150)
                                    Console.WriteLine("Not possible to set FOV higher than 150."); // Force user to stay lower than or equal 150fov
                                else if (n > 65)
                                    Console.WriteLine("Not possible to set FOV lower than 65."); // Force user to stay higher than or equal 65fov
                            }
                        }
                        else if (Input[0].ToLower().Equals("fps")) // If array entry 1 lower case equals "fps" continue
                        {
                            switch (Input[1].ToLower().Equals("default"))
                            {
                                case true:
                                    Console.WriteLine("Setting FPS back to default.");
                                    CommandSet.FPS(); // Set FPS to default
                                    break;
                                case false:
                                    Console.WriteLine("{0} is not a valid argument for FPS, type Help for examples", Input[1]);
                                    break;
                            }
                            if (isNumeric.Equals(true)) // If array entry 2 has int continue
                            {
                                if (n >= 30 && n <= 300)
                                {
                                    Console.WriteLine("Setting FPS to {0}.", Int32.Parse(Input[1]));
                                    CommandSet.FPS(Int32.Parse(Input[1])); // Set FPS to array entry 2
                                }
                                else if (n < 300)
                                    Console.WriteLine("Not possible to set FPS higher than 300."); // Force user to stay lower than or equal 300fps
                                else if (n > 30)
                                    Console.WriteLine("Not possible to set FPS lower than 30."); // Force user to stay higher than or equal 30fps
                            }
                        }
                    }
                }
            }
        }
    }
}
