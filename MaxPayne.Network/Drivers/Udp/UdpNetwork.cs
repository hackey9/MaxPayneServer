using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Network.Drivers.Udp
{
    internal class UdpNetwork : INetwork<IpEndpoint>
    {
        private readonly UdpClient _udp;
        private readonly Thread _sender;
        private readonly Thread _receiver;
        private readonly ConcurrentBag<IMessage<IpEndpoint>> _messagesToSend = new();
        private readonly ConcurrentBag<Message> _messagesToReceive = new();

        private bool _enabled;

        public UdpNetwork()
            : this(new UdpClient(0, AddressFamily.InterNetwork))
        {
        }

        public UdpNetwork(int port)
            : this(new UdpClient(port, AddressFamily.InterNetwork))
        {
        }

        private UdpNetwork(UdpClient udp)
        {
            _udp = udp;
            _sender = new Thread(ProcessSend) {IsBackground = true};
            _receiver = new Thread(ProcessReceive) {IsBackground = true};
        }

        public void Start()
        {
            _enabled = true;
            _sender.Start();
            _receiver.Start();
        }

        public void AddListener(IPAddress ip)
        {
            _udp.JoinMulticastGroup(ip);
        }

        private void ProcessSend()
        {
            Debug.Assert(_enabled);
            while (_enabled)
            {
                while (_messagesToSend.TryTake(out var message))
                {
                    _udp.Send(message.Data.Buffer, message.Data.Length, message.Endpoint.Endpoint);
                }
            }
        }

        private void ProcessReceive()
        {
            Debug.Assert(_enabled);
            while (_enabled)
            {
                try
                {
                    var endpoint = new IPEndPoint(IPAddress.Any, 0);
                    var datagram = _udp.Receive(ref endpoint);

                    _messagesToReceive.Add(new Message(datagram, endpoint));
                }
                catch (SocketException)
                {
                }
            }
        }

        public void Send(IMessage<IpEndpoint> message)
        {
            Debug.Assert(_messagesToSend.Count < 100);

            _messagesToSend.Add(message);
        }

        public IEnumerable<IMessage<IpEndpoint>> ReceiveAll()
        {
            Stack<IMessage<IpEndpoint>> stack = new();

            while (_messagesToReceive.TryTake(out var message))
            {
                stack.Push(message);
            }

            return stack.ToArray();
        }

        public void Dispose()
        {
            _udp.Dispose();
        }
    }
}