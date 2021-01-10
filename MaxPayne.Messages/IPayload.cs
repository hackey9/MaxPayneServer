using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Messages
{
    public interface IPayload
    {
        IpEndpoint Endpoint { get; }
        Datagram ToDatagram();
    }
}