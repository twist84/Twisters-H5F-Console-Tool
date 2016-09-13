using System;

using Memory;

namespace Twisters_H5_Console_Tool
{
    class Get
    {
        public static float FOV()
        {
            return BitConverter.ToSingle(Manager.ReadFromAddress(Addresses.FOV), 0);
        }

        public static int FPS()
        {
            return 1000000 / BitConverter.ToInt16(Manager.ReadFromAddress(Addresses.FPS[0]), 0);
        }
    }

    class Set
    {
        public static void FOV(float newFOV = 78)
        {
            Manager.WriteToAddress(Addresses.FOV, BitConverter.GetBytes(newFOV));
        }

        public static void FPS(float newFPS = 60)
        {
            for (int i = 0; i < Addresses.FPS.Count; i++)
                Manager.WriteToAddress(Addresses.FPS[i], BitConverter.GetBytes(1000000 / Convert.ToInt16(newFPS)));
        }
    }
}
