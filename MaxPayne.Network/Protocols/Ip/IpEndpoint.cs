using System.Net;

namespace MaxPayne.Network.Protocols.Ip
{
    public readonly struct IpEndpoint
    {
        public readonly IPEndPoint Endpoint;
        
        public IpEndpoint(IPEndPoint endpoint)
        {
            Endpoint = endpoint;
        }

        public IpEndpoint(IPAddress ip, int port)
            : this(new IPEndPoint(ip, port))
        {
        }

        public IpEndpoint(string host, int port)
            : this (IPAddress.Parse(host), port)
        {
        }

        public IPAddress Ip => Endpoint.Address;
        public int Port => Endpoint.Port;

        public override bool Equals(object? obj)
        {
            if (obj is IpEndpoint other)
            {
                return Endpoint.Equals(other.Endpoint);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Port;
        }
    }
}
