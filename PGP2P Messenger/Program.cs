using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Unicode;

namespace PGP2P_Messenger
{
    class Program
    {
        public byte[] RSAKey;
        public byte[] RSATargetKey;
        public byte[] AESKey;
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Directory.CreateDirectory("Keys");
            Directory.CreateDirectory("Messages");
            var p = new Program();
            p.UI();
        }
        //need receiver's public key
        public byte[] EncryptStringRSA(string text, byte[] key)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(key, out int bytesRead);
                var data = Encoding.UTF8.GetBytes(text);
                return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            }
        }
        //need private key
        public string DecryptBytesRSA(byte[] data, byte[] key)
        {
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(key, out int bytesRead);
                var ds = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(ds);
            }
        }
        public string EncryptStringAES(string text, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                var stream = new MemoryStream();
                var cryptStream = new CryptoStream(stream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write, true);
                var cStreamWriter = new StreamWriter(cryptStream, Encoding.UTF8, 1024, true);
                cStreamWriter.WriteLine(text);
                cStreamWriter.Close();
                cryptStream.Close();
                var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);
                streamWriter.WriteLine("<END MESSAGE>");
                streamWriter.Close();
                stream.Position = 0;
                var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
                var r = streamReader.ReadToEnd();
                streamReader.Close();
                stream.Close();
                return r;
            }
        }
        public void UI()
        {
            while (true)
            {
                if (RSAKey == null && AESKey == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(">> NO KEY LOADED <<");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine("Initializing PGP2P Messenger");
                Console.WriteLine("0. Configure keys");
                Console.WriteLine("1. Send message");
                Console.WriteLine("2. Receive message");
                Console.WriteLine("3. Quit");
                Console.WriteLine("Please make a selection:");
                var k = Console.ReadKey();
                Console.Clear();
                if (k.Key == ConsoleKey.D0)
                {
                    Console.WriteLine("0. AES");
                    Console.WriteLine("1. RSA");
                    Console.WriteLine("Please make a selection:");
                    var k2 = Console.ReadKey();
                    Console.Clear();
                    if (k2.Key == ConsoleKey.D0)
                    {
                        //check for key file
                        if (false)
                        {

                        }
                        else
                        {
                            Console.WriteLine("No AES key found! Generate new key? Y/N:");
                            var k3 = Console.ReadKey();
                            Console.Clear();
                            if (k3.Key == ConsoleKey.Y)
                            {
                                var c = Aes.Create();
                                c.GenerateKey();
                                AESKey = c.Key;
                                Console.WriteLine("Key saved");
                            }
                        }
                    }
                    bool newKey = false;
                    if (k2.Key == ConsoleKey.D1)
                    {
                        //check for key file
                        if (File.Exists(@"Keys\PRIVATERSAKEY.xkey") && File.Exists(@"Keys\PUBLICRSAKEY.xtkey"))
                        {
                            var keyFilePrivate = new FileStream(@"Keys\PRIVATERSAKEY.xkey", FileMode.Open);
                            var keyFilePublic = new FileStream(@"Keys\PUBLICRSAKEY.xtkey", FileMode.Open);
                            Console.WriteLine("RSA key found. Load key? Y/N:");
                            var k3 = Console.ReadKey();
                            Console.Clear();
                            if (k3.Key == ConsoleKey.Y)
                            {
                                byte[] bytes = new byte[keyFilePrivate.Length];
                                keyFilePrivate.Read(bytes, 0, (int)keyFilePrivate.Length);
                                keyFilePrivate.Close();
                                RSAKey = bytes;
                                byte[] bytes2 = new byte[keyFilePublic.Length];
                                keyFilePublic.Read(bytes2, 0, (int)keyFilePublic.Length);
                                keyFilePublic.Close();
                                RSATargetKey = bytes2;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(">> Key loaded <<");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                Console.WriteLine("Generate new key?");
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(">> WARNING: THIS WILL DELETE YOUR EXISTING KEY <<");
                                Console.WriteLine(">> PRESS THE X KEY FIVE TIMES TO CONFIRM, OR PRESS ESCAPE TO CANCEL <<");
                                var x = 0;
                                while (x < 5)
                                {
                                    var k4 = Console.ReadKey();
                                    if (k4.Key == ConsoleKey.X)
                                    {
                                        x++;
                                        Console.CursorLeft -= 1;
                                        Console.Write("\u2588");
                                        Console.Write("\u2588");
                                    }
                                    else
                                    {
                                        Console.Clear();
                                        break;
                                    }
                                }
                                if (x == 5)
                                {
                                    Console.ForegroundColor = ConsoleColor.White;

                                    Console.Clear();

                                    keyFilePrivate.Close();
                                    keyFilePublic.Close();
                                    File.Delete(@"Keys\PRIVATERSAKEY.xkey");
                                    File.Delete(@"Keys\PUBLICRSAKEY.xkey");
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(">> Key deleted <<");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    newKey = true;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No RSA key found! Generate new key? Y/N:");
                            var k3 = Console.ReadKey();
                            Console.Clear();
                            if (k3.Key == ConsoleKey.Y)
                            {
                                newKey = true;
                            }
                        }
                        if (newKey)
                        {
                            var c = new RSACryptoServiceProvider();
                            RSAKey = c.ExportRSAPrivateKey();
                            RSATargetKey = c.ExportRSAPublicKey();
                            var privateWriter = new FileStream(@"Keys\PRIVATERSAKEY.xkey", FileMode.Create);
                            privateWriter.Write(c.ExportRSAPrivateKey());
                            privateWriter.Close();
                            var publicWriter = new FileStream(@"Keys\PUBLICRSAKEY.xtkey", FileMode.Create);
                            publicWriter.Write(c.ExportRSAPublicKey());
                            publicWriter.Close();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(">> Key created <<");
                            Console.WriteLine(">> Key loaded <<");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }

                }
                if (k.Key == ConsoleKey.D1)
                {
                    Console.WriteLine("0. AES");
                    Console.WriteLine("1. RSA");
                    Console.WriteLine("Please make a selection:");
                    var k2 = Console.ReadKey();
                    Console.Clear();
                    if (k2.Key == ConsoleKey.D0)
                    {
                        Console.WriteLine("Enter message content:");
                        var mc = Console.ReadLine();
                        byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
                        byte[] iv = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
                        var es = EncryptStringAES(mc, key, iv);
                        Console.WriteLine(es);
                    }
                    if (k2.Key == ConsoleKey.D1)
                    {
                        if (RSAKey != null)
                        {
                            Console.WriteLine("Enter message content:");
                            var mc = Console.ReadLine();
                            var es = EncryptStringRSA(mc, RSATargetKey);
                            Console.WriteLine("<BEGIN RSA MESSAGE>");
                            for (int i = 0; i < es.Length; i++)
                            {
                                Console.Write(es[i]);
                            }
                            Console.WriteLine("\n<END RSA MESSAGE>");
                            var ds = DecryptBytesRSA(es, RSAKey);
                            Console.WriteLine("Decryption as follows:");
                            Console.WriteLine("<BEGIN PLAINTEXT MESSAGE>");
                            Console.WriteLine(ds);
                            Console.WriteLine("<END PLAINTEXT MESSAGE>");
                            Console.WriteLine("Press any key to return");
                            Console.ReadKey();
                            Console.Clear();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(">> LOAD A KEY BEFORE ACCESSING ENCRYPTION <<");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                if(k.Key == ConsoleKey.D2)
                {
                    Console.WriteLine("0. Open local file");
                    Console.WriteLine("1. Receive over network");
                    Console.WriteLine("Please make a selection:");
                    var k2 = Console.ReadKey();
                    Console.Clear();
                    if(k2.Key == ConsoleKey.D0)
                    {
                        var files = Directory.GetFiles("Messages",".xtm");
                    }
                }
                if (k.Key == ConsoleKey.D3)
                {
                    break;
                }
            }
        }
    }
}