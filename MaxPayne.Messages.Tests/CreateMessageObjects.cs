using System;
using MaxPayne.Messages.FromClient;
using MaxPayne.Messages.FromServer;
using MaxPayne.Messages.State;
using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MaxPayne.Messages.Tests
{
    [TestClass]
    public class CreateMessageObjects
    {
        private static IpEndpoint ClientEndpoint = new IpEndpoint("50.50.50.50", 5050);
        private static IpEndpoint ServerEndpoint = new IpEndpoint("170.20.40.60", 8000);

        [TestMethod]
        [TestCategory("client")]
        public void CreateClientPayload()
        {
            // send
            const int id = 500;
            var state = new FrameState
            {
                X = 10,
                Y = 20,
                Z = 30.1f,
                RotationAngle = 40.9f,
                VerticalAngle = 17.8f,
                Ammo1 = 4,
                Ammo2 = 261,
                Ammo3 = 400,
                Gun = 0xab,
            };
            IPayload message = new ClientSentFrame(id, state);
            var datagram = message.ToDatagram();

            Assert.AreEqual((byte) MessageTypes.ClientSentFrameState, datagram.Buffer[0]);

            // receive on server
            Assert.IsTrue(MessageFactory.TryFactory(new Message(datagram, ClientEndpoint), out var decodedMessage));

            Assert.IsInstanceOfType(decodedMessage, typeof(ClientSentFrame));

            ClientSentFrame m = decodedMessage as ClientSentFrame;

            Assert.AreEqual(id, m?.ClientId);
            Assert.IsTrue(state.Equals(m?.Frame));
        }

        [TestMethod]
        [TestCategory("client")]
        public void ClientWantsConnect()
        {
            // send
            IPayload message = new ClientWantsConnect();
            var datagram = message.ToDatagram();

            Assert.AreEqual((byte) MessageTypes.ClientWantsConnectToServer, datagram.Buffer[0]);

            // receive on server
            Assert.IsTrue(MessageFactory.TryFactory(new Message(datagram, ClientEndpoint), out var decodedMessage));

            Assert.IsInstanceOfType(decodedMessage, typeof(ClientWantsConnect));
            Assert.AreEqual(ClientEndpoint, (decodedMessage as ClientWantsConnect)?.Endpoint);
        }

        [TestMethod]
        [TestCategory("server")]
        public void ServerBroadcast()
        {
            // send
            IPayload message = new BroadcastServerIsHere();
            var datagram = message.ToDatagram();

            Assert.AreEqual((byte) MessageTypes.BroadcastServerIsHere, datagram.Buffer[0]);

            // receive on client
            Assert.IsTrue(MessageFactory.TryFactory(new Message(datagram, ServerEndpoint), out var receivedMessage));

            Assert.IsInstanceOfType(receivedMessage, typeof(BroadcastServerIsHere));
            Assert.AreEqual(ServerEndpoint, (receivedMessage as BroadcastServerIsHere)?.Endpoint);
        }

        [TestMethod]
        [TestCategory("server")]
        public void ClientConnected()
        {
            const int id = 123;

            // send
            IPayload message = new ClientConnected(id);
            var datagram = message.ToDatagram();

            Assert.AreEqual((byte) MessageTypes.ClientConnectedToServer, datagram.Buffer[0]);

            // receive on client
            Assert.IsTrue(MessageFactory.TryFactory(new Message(datagram, ServerEndpoint), out var receivedMessage));

            Assert.IsInstanceOfType(receivedMessage, typeof(ClientConnected));
            Assert.AreEqual(id, (receivedMessage as ClientConnected)?.ClientId);
            Assert.AreEqual(ServerEndpoint, (receivedMessage as ClientConnected)?.Endpoint);
        }

        [TestMethod]
        [TestCategory("server")]
        public void FuckYou()
        {
            var id = 100500;

            // send
            IPayload message = new GetOutOfHere(id);
            var datagram = message.ToDatagram();

            Assert.AreEqual((byte) MessageTypes.GetTheFuckOut, datagram.Buffer[0]);

            // receive on client

            MessageFactory.TryFactory(new Message(datagram, ServerEndpoint), out var receivedMessage);

            Assert.IsInstanceOfType(receivedMessage, typeof(GetOutOfHere));
            Assert.AreEqual(id, (receivedMessage as GetOutOfHere)?.ClientId);
            Assert.AreEqual(ServerEndpoint, (receivedMessage as GetOutOfHere)?.Endpoint);
        }

        [TestMethod]
        public void ServerSentState()
        {
            var player1 = new PlayerState()
            {
                Id = 10, Gun = 0xac,
                X = 1, Y = 1, Z = 1,
                RotationAngle = 30, VerticalAngle = 10,
            };
            var player2 = new PlayerState()
            {
                Id = 12, Gun = 0xac,
                X = 2, Y = 2, Z = 2,
                RotationAngle = 30, VerticalAngle = 10,
            };
            var state = new GameState()
            {
                Players = new[] {player1, player2},
            };

            // send
            IPayload message = new ServerSentGameState(state);
            var datagram = message.ToDatagram();

            Assert.AreEqual((byte) MessageTypes.ServerSendGameState, datagram.Buffer[0]);

            // receive on client
            Assert.IsTrue(MessageFactory.TryFactory(new Message(datagram, ServerEndpoint), out var receivedMessage));
            Assert.IsInstanceOfType(receivedMessage, typeof(ServerSentGameState));
            var m = (ServerSentGameState) receivedMessage;
            
            Assert.AreEqual(2, m.State.Players.Length);
            Assert.AreEqual(player1, m.State.Players[0]);
            Assert.AreEqual(player2, m.State.Players[1]);
            Assert.AreEqual(ServerEndpoint, m.Endpoint);
        }
    }
}