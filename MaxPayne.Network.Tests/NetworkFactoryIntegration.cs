using System.Linq;
using System.Text;
using System.Threading;
using MaxPayne.Network.Protocols.Ip.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MaxPayne.Network.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class NetworkFactoryIntegration
    {
        [TestMethod]
        public void ServerSendToClientExample()
        {
            var broadcastEndpoint = NetworkFactory.UdpBroadcastEndpoint;
            using var client = NetworkFactory.UdpClient(broadcastEndpoint);

            Thread.Sleep(100);

            var count = 5;
            var textSent = "meow!";
            var datagram = Encoding.ASCII.GetBytes(textSent);

            {
                using var server = NetworkFactory.UdpServer();
                for (int i = 0; i < count; i++)
                {
                    server.Send(new Message(datagram, broadcastEndpoint));
                }
            }

            Thread.Sleep(100);

            var messages = client.ReceiveAll();
            var receivedCount = 0;
            foreach (var receivedMessage in messages)
            {
                var textReceived = Encoding.ASCII.GetString(receivedMessage.Data.Buffer);

                Assert.AreEqual(textSent, textReceived);
                receivedCount++;
            }

            Assert.AreEqual(count, receivedCount);
        }

        [TestMethod]
        public void ClientRespondToServer()
        {
            // connect
            using var server = NetworkFactory.UdpServer();
            using var client = NetworkFactory.UdpClient(NetworkFactory.UdpBroadcastEndpoint);

            Thread.Sleep(100);

            // server broadcast
            var textSent = "kar!";
            {
                server.Send(new Message(ToBytes(textSent), NetworkFactory.UdpBroadcastEndpoint));
                Thread.Sleep(100);
            }

            // client receive
            var messageReceived = client.ReceiveAll().First();
            Assert.AreEqual(textSent, Stringify(messageReceived.Data.Buffer));

            Thread.Sleep(100);

            // client respond
            var textRespond = "meow!";
            {
                client.Send(new Message(ToBytes(textRespond), messageReceived.Endpoint));
                Thread.Sleep(100);
            }

            // server accept
            var respondMessage = server.ReceiveAll().First();
            Assert.AreEqual(textRespond, Stringify(respondMessage.Data.Buffer));

            // server send direct
            var directText = "rrr!";
            {
                server.Send(new Message(ToBytes(directText), respondMessage.Endpoint));
                Thread.Sleep(100);
            }

            // client accept
            var directMessage = client.ReceiveAll().First();
            Assert.AreEqual(directText, Stringify(directMessage.Data.Buffer));
        }

        private static string Stringify(byte[] bytes) => Encoding.ASCII.GetString(bytes);
        private static byte[] ToBytes(string str) => Encoding.ASCII.GetBytes(str);
    }
}