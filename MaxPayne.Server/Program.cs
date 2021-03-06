﻿using System;
using System.IO;
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

            var port = LoadConfig("config-server.txt");

            if (port is not null)
            {
                Console.WriteLine($"Use configured port {port}");
            }

            using var app = port is null
                ? new App()
                : new App(port.Value);
            app.Run();

            Console.ReadKey();
        }

        private static int? LoadConfig(string file)
        {
            if (!File.Exists(file)) return null;

            var portString = File.ReadAllText(file);
            return int.Parse(portString);
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