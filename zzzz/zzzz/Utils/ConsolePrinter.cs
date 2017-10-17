using System;

namespace zzzz
{
    public static class ConsolePrinter
    {
        private static float lastPrintTime;

        public static void Print(string str)
        {
            //return;

            var timeDiff = EvadeUtils.TickCount - lastPrintTime;

            var finalStr = "[" + timeDiff + "] " + str;

            Console.WriteLine(finalStr);

            lastPrintTime = EvadeUtils.TickCount;
        }
    }
}