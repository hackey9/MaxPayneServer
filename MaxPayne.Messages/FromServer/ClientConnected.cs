using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Messages.FromServer
{
    public class ClientConnected : IPayload
    {
        public IpEndpoint Endpoint { get; }
        public int ClientId { get; }

        public ClientConnected(IpEndpoint endpoint, int clientId)
        {
            Endpoint = endpoint;
            ClientId = clientId;
        }

        public ClientConnected(IpEndpoint endpoint, BinaryReader stream)
            : this(endpoint, stream.ReadInt32())
        {
        }

        public ClientConnected(int clientId)
        {
            ClientId = clientId;
        }

        public Datagram ToDatagram() => new Datagram(stream =>
        {
            stream.Write((byte)MessageTypes.ClientConnectedToServer);
            stream.Write(ClientId);
        });
    }
}