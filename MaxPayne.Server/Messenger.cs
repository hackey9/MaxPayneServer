using MaxPayne.Messages;
using MaxPayne.Messages.FromServer;
using MaxPayne.Messages.State;
using MaxPayne.Network;
using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Server
{
    public class Messenger
    {
        private readonly INetwork<IpEndpoint> _network;

        public Messenger(INetwork<IpEndpoint> network)
        {
            _network = network;
        }

        private void Send(IPayload payload, IpEndpoint endpoint)
        {
            var message = new Message(payload.ToDatagram(), endpoint);
            _network.Send(message);
        }
        
        public void ClientConnected(int id, IpEndpoint endpoint) 
            => Send(new ClientConnected(id), endpoint);

        public void Broadcast(IpEndpoint endpoint) 
            => Send(new BroadcastServerIsHere(), endpoint);

        public void FuckYou(int id, IpEndpoint endpoint) 
            => Send(new GetOutOfHere(id), endpoint);

        public void GameState(GameState state, IpEndpoint endpoint) 
            => Send(new ServerSentGameState(state), endpoint);
    }
}