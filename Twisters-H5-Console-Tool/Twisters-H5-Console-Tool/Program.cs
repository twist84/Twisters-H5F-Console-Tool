﻿using System;
using System.Collections.Generic;

using Memory;

namespace Twisters_H5_Console_Tool
{
    class Program
    {
        static Int32 FOV_Address = 0x58ECF90; // Global FOV memory address
        static List<Int32> FPS_Address = new List<Int32>(new Int32[] { 0x34B8C50, 0x34B8C60, 0x34B8C70 }); // Global FPS memory addresses

        static void Main(string[] args)
        {
            Console.WriteLine("Current FOV: {0}", FOV_Get()); // Tell user current FOV
            Console.WriteLine("Current FPS: {0}", FPS_Get()); // Tell user current FPS

            while (true) // Main loop
            {
                Console.Write("Enter a Command or Type Help to Show help");
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
                            FOV_Set(Int32.Parse(Input[1])); // Set FOV to array entry 2
                        }
                        else if (Input[1].ToLower().Equals("default")) // If array entry 2 lower case equals "default" continue
                        {
                            Console.WriteLine("Setting FOV back to default.");
                            FOV_Set(); // Set FOV to default
                        }
                    }
                    else if (Input[0].ToLower().Equals("fps")) // If array entry 1 lower case equals "fps" continue
                    {
                        if (isNumeric.Equals(true)) // If array entry 2 has int continue
                        {
                            Console.WriteLine("Setting FPS to {0}.", Int32.Parse(Input[1]));
                            FPS_Set(Int32.Parse(Input[1])); // Set FPS to array entry 2
                        }
                        else if (Input[1].ToLower().Equals("default")) // If array entry 2 lower case equals "default" continue
                        {
                            Console.WriteLine("Setting FPS back to default.");
                            FPS_Set(); // Set FPS to default
                        }
                    }
                }
            }
        }

        static float FOV_Get()
        {
            return BitConverter.ToSingle(Manager.ReadFromAddress(FOV_Address), 0);
        }

        static int FPS_Get()
        {
            return 1000000 / BitConverter.ToInt16(Manager.ReadFromAddress(0x34B8C50), 0);
        }

        static void FOV_Set(float newFOV = 78)
        {
            Manager.WriteToAddress(FOV_Address, BitConverter.GetBytes(newFOV));
        }

        static void FPS_Set(float newFPS = 60)
        {
            for (int i = 0; i < FPS_Address.Count; i++)
                Manager.WriteToAddress(FPS_Address[i], BitConverter.GetBytes(1000000 / Convert.ToInt16(newFPS)));
        }
    }
}