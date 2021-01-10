using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Messages.FromClient
{
    public class ClientWantsConnect : IPayload
    {
        public IpEndpoint Endpoint { get; }

        public ClientWantsConnect(IpEndpoint clientEndpoint)
        {
            Endpoint = clientEndpoint;
        }

        public ClientWantsConnect()
        {
        }

        public Datagram ToDatagram()
        {
            return new Datagram(stream =>
            {
                stream.Write((byte)MessageTypes.ClientWantsConnectToServer);
            });
        }
    }
}
