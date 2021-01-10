using System.IO;
using MaxPayne.Messages.State;
using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Messages.FromClient
{
    public class ClientSentFrame : IPayload
    {
        public IpEndpoint Endpoint { get; }
        public int ClientId { get; }
        public FrameState Frame { get; }

        public ClientSentFrame(IpEndpoint endpoint, BinaryReader stream)
        {
            Endpoint = endpoint;
            ClientId = stream.ReadInt32();
            Frame = new FrameState(stream);
        }

        public ClientSentFrame(int clientId, FrameState state)
        {
            ClientId = clientId;
            Frame = state;
        }

        public Datagram ToDatagram() => new Datagram(stream =>
        {
            stream.Write((byte) MessageTypes.ClientSentFrameState);
            stream.Write(ClientId);
            Frame.Export(stream);
        });
    }
}