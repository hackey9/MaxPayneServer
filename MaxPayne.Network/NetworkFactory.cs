using System.Net;
using MaxPayne.Network.Drivers.Udp;
using MaxPayne.Network.Protocols.Ip;

namespace MaxPayne.Network
{
    public static class NetworkFactory
    {
        private static object _sync = new();

        public static INetwork<IpEndpoint> UdpClient(IpEndpoint broadcast)
        {
            lock (_sync)
            {
                var network = new UdpNetwork(broadcast.Port);
                network.AddListener(broadcast.Ip);
                network.Start();
                return network;
            }
        }

        public static INetwork<IpEndpoint> UdpClient()
        {
            var network = new UdpNetwork(0);
            network.Start();
            return network;
        }

        public static IpEndpoint UdpBroadcastEndpoint => new IpEndpoint("230.0.0.1", 8080);

        public static INetwork<IpEndpoint> UdpServer()
        {
            lock (_sync)
            {
                var network = new UdpNetwork();
                network.Start();

                return network;
            }
        }

        public static INetwork<IpEndpoint> UdpServer(int port)
        {
            lock (_sync)
            {
                var network = new UdpNetwork(new IPEndPoint(IPAddress.Any, port));
                network.Start();

                return network;
            }
        }
    }
}