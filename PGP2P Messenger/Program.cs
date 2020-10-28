using System;

namespace PGP2P_Messenger
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.Init();
        }
        public void Init()
        {
            Console.WriteLine("Initializing PGP2P Messenger");
            Console.WriteLine("1. Send message");
            Console.WriteLine("2. Receive message");
            Console.WriteLine("Please make a selection:");
            var k = Console.ReadKey();
            if(k.Key == ConsoleKey.D1)
            {
                Console.Clear();
                Console.WriteLine("Enter message content:");
                var mc = Console.ReadLine();
            }
        }
    }
}
