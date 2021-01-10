using System.Net;

namespace MaxPayne.Network.Protocols.Ip.Data
{
    public readonly struct Message : IMessage<IpEndpoint>
    {
        public IData Data { get; }
        public IpEndpoint Endpoint { get; }

        public Message(Datagram data, IpEndpoint endpoint)
        {
            Data = data;
            Endpoint = endpoint;
        }

        public Message(byte[] datagram, IPEndPoint endpoint)
            : this(new Datagram(datagram), new IpEndpoint(endpoint))
        {
        }

        public Message(byte[] datagram, IpEndpoint endpoint)
            : this(new Datagram(datagram), endpoint)
        {
        }
        public Message(Datagram datagram, IPEndPoint endpoint)
            : this(datagram, new IpEndpoint(endpoint))
        {
        }
    }
}