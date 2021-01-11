using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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
        private const int FrameLifetime = 200;

        private readonly INetwork<IpEndpoint> _network;
        private readonly Thread _receiverThread;
        private readonly Thread _senderThread;

        private readonly Stopwatch _frameWatch = new();
        private readonly object _gameSync = new();
        private readonly object _frameSync = new();
        private FrameState _frame;
        private GameState _game = new GameState {Players = Array.Empty<PlayerState>()};

        private IpEndpoint? _serverEndpoint;
        private int? _serverId;
        private readonly Stopwatch _serverWatch = new();
        private readonly bool _serverConfigured;

        public Backend()
        {
            _serverEndpoint = LoadConfig("config-client.txt");
            _serverConfigured = _serverEndpoint is not null;

            _network = GetNetwork(useBroadcast: !_serverConfigured);
            _receiverThread = new Thread(ProcessReceive) {IsBackground = true};
            _senderThread = new Thread(ProcessSend) {IsBackground = true};

            if (_serverEndpoint is not null)
            {
                Console.WriteLine($"Client configured to server {_serverEndpoint.Value.Endpoint}");
            }
        }

        private static INetwork<IpEndpoint> GetNetwork(bool useBroadcast)
            => useBroadcast
                ? NetworkFactory.UdpClient(NetworkFactory.UdpBroadcastEndpoint)
                : NetworkFactory.UdpClient();

            private static IpEndpoint? LoadConfig(string file)
        {
            if (!File.Exists(file)) return null;

            var configLine = File.ReadAllText(file);

            return new IpEndpoint(IPEndPoint.Parse(configLine));
        }

        public void Start()
        {
            Console.WriteLine("Started");
            _receiverThread.Start();
            _senderThread.Start();

            if (_serverConfigured)
            {
                SendIWantToConnect(_serverEndpoint!.Value);
            }
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
                    if (_frameWatch.ElapsedMilliseconds < FrameLifetime)
                    {
                        var payload = new ClientSentFrame(_serverId.Value, _frame);
                        var message = new Message(payload.ToDatagram(), _serverEndpoint.Value);

                        _network.Send(message);
                        Console.Write('-');
                    }
                }

                var sleep = SendStateDelay - (int) requestWatch.ElapsedMilliseconds;

                Thread.Sleep(sleep < 1 ? 1 : sleep);
            }
        }

        private void DisconnectFromServer()
        {
            Console.WriteLine();
            Console.WriteLine("Disconnected");
            // BUG race
            if (_serverConfigured)
            {
                _serverId = null;
                _serverWatch.Restart();
                SendIWantToConnect(_serverEndpoint!.Value);
            }
            else
            {
                _serverId = null;
                _serverEndpoint = null;
                _serverWatch.Reset();
            }

            
        }

        private void ProcessReceive()
        {
            while (true)
            {
                foreach (var networkMessage in _network.ReceiveAll())
                {
                    if (!MessageFactory.TryFactory(networkMessage, out var message)) continue;

                    if (_serverEndpoint is not null)
                    {
                        if (!_serverEndpoint.Equals(message.Endpoint))
                            continue;
                    }

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

                    // update server timer
                    if (_serverEndpoint is not null)
                    {
                        _serverWatch.Restart();
                    }
                } // foreach message

                Thread.Sleep(5);
            } // while true
        }

        private void HandleNewGameState(GameState state, IpEndpoint endpoint)
        {
            if (!_serverEndpoint.Equals(endpoint))
            {
                Debugger.Break();
                return;
            }

            Console.Write(state.Players.Length); 
            lock (_gameSync)
            {
                _game = state;
            }
        }

        private void HandleConnected(int clientId, IpEndpoint serverEndpoint)
        {
            if (_serverEndpoint is null) return;

            // if we have not connected as client
            if (_serverId is not null) return;

            // if servers are match
            if (!_serverEndpoint.Value.Equals(serverEndpoint))
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"Connected as {clientId}");
            _serverId = clientId;
        }

        private void HandleFuck(int clientId, IpEndpoint serverEndpoint)
        {
            if (_serverEndpoint is null || _serverId is null) return;

            if (_serverId.Value != clientId) return;

            if (serverEndpoint.Equals(_serverEndpoint.Value))
            {
                Console.WriteLine();
                Console.WriteLine("Disconnected by server");
                DisconnectFromServer();
            }
        }

        private void HandleServerBroadcast(IpEndpoint serverEndpoint)
        {
            if (_serverConfigured) return;

            if (_serverEndpoint is not null) return;
            Console.WriteLine();
            Console.WriteLine($"Broadcast taken {serverEndpoint.Endpoint}");

            _serverEndpoint = serverEndpoint;
            SendIWantToConnect(serverEndpoint);
        }

        private void SendIWantToConnect(IpEndpoint serverEndpoint)
        {
            Console.WriteLine();
            Console.WriteLine("Try to connect...");
            _serverWatch.Restart();
            var payload = new ClientWantsConnect();
            var message = new Message(payload.ToDatagram(), serverEndpoint);
            _network.Send(message);
        }
    }
}