using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MaxPayne.Client.Application;
using MaxPayne.Messages.State;

namespace MaxPayneMod.ExampleAppCSharp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            await Task.Delay(1000);
            var clientThread = StartClient();


            Enabled = true;

            Console.WriteLine("Running...");
            //do
            //{
            //    Console.WriteLine("Press [q] to exit...");
            //} while (Console.ReadKey(true).Key != ConsoleKey.Q);

            //Enabled = false;

            //Console.WriteLine("Wait to exit...");
            clientThread.Join();
            Console.WriteLine("Exited.");
        }

        private static Thread StartClient()
        {
            var thread = new Thread(ProcessClient)
            {
                Name = "Client",
                IsBackground = true,
            };
            thread.Start();
            return thread;
        }

        private static bool Enabled { get; set; }

        private static void ProcessClient()
        {
            var watch = Stopwatch.StartNew();
            var rand = new Random();

            ClientApp.OnAttach();
            while (Enabled)
            {
                // high frequency of frames
                var gameState = ClientApp.OnFrame(new FrameState
                {
                    X = rand.Next(),
                    Y = rand.Next(),
                    Z = rand.Next(),
                    RotationAngle = (float) rand.NextDouble(),
                    VerticalAngle = (float) rand.NextDouble(),
                    Gun = (byte) rand.Next(0, 255),
                    Ammo1 = (short) rand.Next(0, 1000),
                    Ammo2 = (short) rand.Next(0, 1000),
                    Ammo3 = (short) rand.Next(0, 1000),
                });
                //var players = gameState.Players.Length;

                if (watch.ElapsedMilliseconds > 12000)
                {
                    Console.WriteLine("Sleep 3s (Game pause test)");
                    Thread.Sleep(3000);
                    watch.Restart();
                }
            }

            ClientApp.OnDetach();
        }
    }
}