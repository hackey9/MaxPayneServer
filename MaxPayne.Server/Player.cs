using System.Diagnostics;
using MaxPayne.Messages.State;
using MaxPayne.Network.Protocols.Ip;

namespace MaxPayne.Server
{
    internal class Player
    {
        public readonly IpEndpoint Endpoint;
        public readonly Stopwatch Watch;

        private FrameState? _state;

        public FrameState? State
        {
            get => _state;
            set
            {
                Watch.Restart();
                _state = value;
            }
        }

        public Player(IpEndpoint endpoint)
        {
            Endpoint = endpoint;
            Watch = Stopwatch.StartNew();
            _state = null;
        }

        public bool MustBeDisconnect(int timeToDelay) => Watch.ElapsedMilliseconds > timeToDelay;
    }
}