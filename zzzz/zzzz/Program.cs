using System;

namespace zzzz
{
    internal class Program
    {
        private static Evade evade;

        private static void Main(string[] args)
        {
            try
            {
                evade = new Evade();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}