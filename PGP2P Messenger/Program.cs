using System;
using System.IO;
using System.Security.Cryptography;

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
            if (k.Key == ConsoleKey.D1)
            {
                Console.Clear();
                Console.WriteLine("Enter message content:");
                var mc = Console.ReadLine();

                var aes = Aes.Create();
                byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
                byte[] iv = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
                var stream = new MemoryStream();
                var cryptStream = new CryptoStream(stream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write,true);
                var cStreamWriter = new StreamWriter(cryptStream, System.Text.Encoding.UTF8, 1024, true);
                cStreamWriter.WriteLine(mc);
                cStreamWriter.Close();
                cryptStream.Close();
                var streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8, 1024, true);
                streamWriter.WriteLine("<END MESSAGE>");
                streamWriter.Close();
                stream.Position = 0;
                var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8, true, 1024, true);
                Console.WriteLine(streamReader.ReadToEnd());
                streamReader.Close();
                stream.Close();
                Console.ReadLine();
            }
        }
    }
}
