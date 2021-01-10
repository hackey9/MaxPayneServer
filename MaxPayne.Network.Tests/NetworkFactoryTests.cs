using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MaxPayne.Network.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class NetworkFactoryTests
    {
        [TestMethod]
        public void BroadcastEndpoint()
        {
            var endpoint = NetworkFactory.UdpBroadcastEndpoint;

            Assert.AreEqual(endpoint.Ip, IPAddress.Parse("230.0.0.1"));
            Assert.AreEqual(endpoint.Port, 8080);
        }

        [TestMethod]
        public void UdpServerCreates()
        {
            using var server = NetworkFactory.UdpServer();
            Assert.IsNotNull(server);
        }

        [TestMethod]
        public void UdpClientCreates()
        {
            var broadcastEndpoint = NetworkFactory.UdpBroadcastEndpoint;
            using var client = NetworkFactory.UdpClient(broadcastEndpoint);

            Assert.IsNotNull(client);
        }
    }
}