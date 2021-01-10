using System.IO;
using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Messages.FromServer
{
    public class GetOutOfHere : IPayload
    {
        public int ClientId { get; }
        public IpEndpoint Endpoint { get; }

        public GetOutOfHere(int clientId)
        {
            ClientId = clientId;
        }

        public GetOutOfHere(IpEndpoint endpoint, BinaryReader stream)
        {
            Endpoint = endpoint;
            ClientId = stream.ReadInt32();
        }
        
        Datagram IPayload.ToDatagram()
        {
            return new(writer =>
            {
                writer.Write((byte)MessageTypes.GetTheFuckOut);
                writer.Write(ClientId);
            });
        }
    }
}