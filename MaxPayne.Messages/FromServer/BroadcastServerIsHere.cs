using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Messages.FromServer
{
    public class BroadcastServerIsHere : IPayload
    {
        public IpEndpoint Endpoint { get; }

        public BroadcastServerIsHere(IpEndpoint endpoint)
        {
            Endpoint = endpoint;
        }

        public BroadcastServerIsHere()
        {
        }

        public Datagram ToDatagram()
        {
            return new Datagram(new[]
            {
                (byte) MessageTypes.BroadcastServerIsHere
            });
        }
    }
}