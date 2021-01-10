using System;
using System.Threading;
using MaxPayne.Messages;
using MaxPayne.Messages.FromServer;
using MaxPayne.Network;

namespace MaxPayne.Server
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using var app = new App();
            app.Run();

            Console.ReadKey();
        }

        private static void RunClient()
        {
            using var client = NetworkFactory.UdpClient(NetworkFactory.UdpBroadcastEndpoint);

            while (true)
            {
                foreach (var networkMessage in client.ReceiveAll())
                {
                    if (!MessageFactory.TryFactory(networkMessage, out var message)) continue;

                    if (message is BroadcastServerIsHere broadcast)
                    {
                        Console.WriteLine(message.Endpoint.Endpoint.ToString());
                    }
                }
            }
        }
    }
}