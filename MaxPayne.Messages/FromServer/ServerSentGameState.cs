using System.IO;
using MaxPayne.Messages.State;
using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Messages.FromServer
{
    public class ServerSentGameState : IPayload
    {
        public IpEndpoint Endpoint { get; }
        public GameState State { get; }

        public ServerSentGameState(IpEndpoint endpoint, BinaryReader stream)
        {
            Endpoint = endpoint;
            State = new GameState(stream);
        }

        public ServerSentGameState(GameState state)
        {
            State = state;
        }

        Datagram IPayload.ToDatagram()
        {
            return new(writer =>
            {
                writer.Write((byte) MessageTypes.ServerSendGameState);
                State.Export(writer);
            });
        }
    }
}