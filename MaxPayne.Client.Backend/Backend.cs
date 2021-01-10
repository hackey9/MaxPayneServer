using System;
using System.Diagnostics;
using System.Threading;
using MaxPayne.Messages;
using MaxPayne.Messages.FromClient;
using MaxPayne.Messages.FromServer;
using MaxPayne.Messages.State;
using MaxPayne.Network;
using MaxPayne.Network.Protocols.Ip;
using MaxPayne.Network.Protocols.Ip.Data;

namespace MaxPayne.Client.Backend
{
    public sealed class Backend : IBackend
    {
        private const int SendStateDelay = 50;
        private const int DisconnectServerDelay = 1000;

        private readonly INetwork<IpEndpoint> _network;
        private readonly Thread _receiverThread;
        private readonly Thread _senderThread;

        private readonly Stopwatch _frameWatch = new();
        private readonly object _gameSync = new();
        private readonly object _frameSync = new();
        private FrameState _frame;
        private GameState _game;

        private IpEndpoint? _serverEndpoint;
        private int? _serverId;
        private Stopwatch _serverWatch = new();

        public Backend()
        {
            _network = NetworkFactory.UdpClient(NetworkFactory.UdpBroadcastEndpoint);
            _receiverThread = new Thread(ProcessReceive) {IsBackground = true};
            _senderThread = new Thread(ProcessSend) {IsBackground = true};
        }

        public void Start()
        {
            Console.WriteLine("Started");
            _receiverThread.Start();
            _senderThread.Start();
        }

        public GameState Frame(FrameState state)
        {
            _frameWatch.Restart();

            lock (_frameSync)
            {
                _frame = state;
            }

            lock (_gameSync)
            {
                return _game;
            }
        }

        public void Dispose()
        {
            _network.Dispose();
        }

        private void ProcessSend()
        {
            var requestWatch = new Stopwatch();
            while (true)
            {
                requestWatch.Restart();

                // if (_frameWatch.ElapsedMilliseconds >= DisconnectDelay)
                // {
                //     DisconnectFromServer(); // because frames are stopped
                // }
                // FIX: Disconnect will proceed automatically by server
                //      a) client received Fuck message
                //      b) client will disconnect from server because delay

                //  b) is there:
                if (_serverWatch.ElapsedMilliseconds >= DisconnectServerDelay)
                {
                    DisconnectFromServer(); // because server was not respond
                }

                if (_serverId is not null && _serverEndpoint is not null)
                {
                    var payload = new ClientSentFrame(_serverId.Value, _frame);
                    var message = new Message(payload.ToDatagram(), _serverEndpoint.Value);

                    _network.Send(message);
                }

                var sleep = SendStateDelay - (int) requestWatch.ElapsedMilliseconds;
                if (sleep > 0)
                {
                    Thread.Sleep(sleep);
                }
            }
        }

        private void DisconnectFromServer()
        {
            // BUG race

            Console.WriteLine("Disconnected");
            _serverId = null;
            _serverEndpoint = null;
            _serverWatch.Reset();
        }

        private void ProcessReceive()
        {
            while (true)
            {
                foreach (var networkMessage in _network.ReceiveAll())
                {
                    if (!MessageFactory.TryFactory(networkMessage, out var message)) continue;
                    
                    switch (message)
                    {
                        case BroadcastServerIsHere broadcastMessage:
                            HandleServerBroadcast(broadcastMessage.Endpoint);
                            break;
                        case GetOutOfHere fuck:
                            HandleFuck(fuck.ClientId, fuck.Endpoint);
                            break;
                        case ClientConnected connectedMessage:
                            HandleConnected(connectedMessage.ClientId, connectedMessage.Endpoint);
                            break;
                        case ServerSentGameState stateMessage:
                            HandleNewGameState(stateMessage.State, stateMessage.Endpoint);
                            break;
                    }

                    Console.Write('+');

                    // update server timer
                    if (_serverEndpoint is not null && _serverEndpoint.Value.Equals(message.Endpoint))
                    {
                        _serverWatch.Restart();
                    }
                } // foreach message
            } // while true
        }

        private void HandleNewGameState(GameState state, IpEndpoint endpoint)
        {
            if (!_serverEndpoint.Equals(endpoint))
            {
                Debugger.Break();
                return;
            }

            lock (_gameSync)
            {
                _game = state;
            }
        }

        private void HandleConnected(int clientId, IpEndpoint serverEndpoint)
        {
            Console.WriteLine("Connect request...");
            if (_serverEndpoint is null) return;

            // if we have not connected as client
            if (_serverId is not null) return;

            // if servers are match
            if (!_serverEndpoint.Value.Equals(serverEndpoint)) return;

            Console.WriteLine($"Connected as {clientId}");
            _serverId = clientId;
        }

        private void HandleFuck(int clientId, IpEndpoint serverEndpoint)
        {
            Console.WriteLine("Disconnect request...");
            if (_serverEndpoint is null || _serverId is null) return;

            if (_serverId.Value != clientId) return;

            if (serverEndpoint.Equals(_serverEndpoint.Value))
            {
                Console.WriteLine("Disconnected");
                DisconnectFromServer();
            }
        }

        private void HandleServerBroadcast(IpEndpoint serverEndpoint)
        {
            Console.WriteLine($"Broadcast taken {serverEndpoint.Endpoint}");
            if (_serverEndpoint is not null) return;

            _serverEndpoint = serverEndpoint;

            Console.WriteLine("Try to connect...");
            var payload = new ClientWantsConnect();
            var message = new Message(payload.ToDatagram(), serverEndpoint);
            _network.Send(message);
        }
    }
}