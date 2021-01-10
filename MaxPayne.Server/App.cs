using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MaxPayne.Messages;
using MaxPayne.Messages.FromClient;
using MaxPayne.Messages.State;
using MaxPayne.Network;
using MaxPayne.Network.Protocols.Ip;

namespace MaxPayne.Server
{
    public sealed class App : IDisposable
    {
        public int BroadcastDelay { get; set; } = 1000;
        public int SendDelay { get; set; } = 50;
        public int PlayerDisconnectDelay { get; set; } = 1500;

        private readonly INetwork<IpEndpoint> _network;
        private readonly Thread _broadcastThread;
        private readonly Thread _sendThread;
        private readonly Thread _receiveThread;
        private readonly ConcurrentDictionary<int, Player> _clients = new();
        private readonly Messenger _messenger;

        private int _lastId;

        public App()
        {
            _network = NetworkFactory.UdpServer();
            _messenger = new Messenger(_network);
            _broadcastThread = new Thread(ProcessBroadcast)
            {
                IsBackground = true,
                Name = "Broadcast",
            };
            _sendThread = new Thread(ProcessSend)
            {
                IsBackground = true,
                Name = "Send",
            };
            _receiveThread = new Thread(ProcessReceive)
            {
                IsBackground = true,
                Name = "Receive",
            };
        }

        private void ProcessReceive()
        {
            while (true)
            {
                foreach (var networkMessage in _network.ReceiveAll())
                {
                    if (!MessageFactory.TryFactory(networkMessage, out var payload)) continue;

                    switch (payload)
                    {
                        case ClientSentFrame frame:
                            HandleFrame(frame.ClientId, frame.Frame, frame.Endpoint);
                            break;
                        case ClientWantsConnect connect:
                            HandleConnect(connect.Endpoint);
                            break;
                    }
                }
            }
        }

        private void HandleFrame(int clientId, FrameState frame, IpEndpoint endpoint)
        {
            if (!_clients.ContainsKey(clientId)) return;

            // BUG: race (critical)
            if (!_clients[clientId].Endpoint.Equals(endpoint)) return;
            Console.WriteLine($"{clientId} sent frame");

            _clients[clientId].State = frame;
        }

        private void HandleConnect(IpEndpoint clientEndpoint)
        {
            var id = _lastId++;
            Console.WriteLine($"{id} connected");
            _clients[id] = new Player(clientEndpoint);

            _messenger.ClientConnected(id, clientEndpoint);
        }

        private void ProcessSend()
        {
            var watch = new Stopwatch();
            while (true)
            {
                watch.Restart();

                DisconnectAfkPlayers();

                var state = MakeGameState();

                foreach (var (id, player) in _clients)
                {
                    _messenger.GameState(state, player.Endpoint);
                }
                //Debug.WriteLine("State built and sent");

                var sleep = SendDelay - (int) watch.ElapsedMilliseconds;
                if (sleep > 0)
                {
                    Thread.Sleep(sleep);
                }
            }
        }

        private void DisconnectAfkPlayers()
        {
            foreach (var id in _clients.Keys.ToArray())
            {
                if (!_clients.TryGetValue(id, out var player)) continue;

                if (!player.MustBeDisconnect(PlayerDisconnectDelay)) continue;

                if (_clients.TryRemove(id, out player))
                {
                    Console.WriteLine($"{id} disconnected");
                    _messenger.FuckYou(id, player!.Endpoint);
                }
            }
        }

        private GameState MakeGameState()
        {
            List<PlayerState> players = new();
            foreach (var (id, player) in _clients)
            {
                if (player.State is null) continue;
                
                var state = player.State.Value;
                players.Add(new PlayerState
                {
                    Id = id,
                    X = state.X,
                    Y = state.Y,
                    Z = state.Z,
                    Gun = state.Gun,
                    RotationAngle = state.RotationAngle,
                    VerticalAngle = state.VerticalAngle,
                });
            }
            
            return new GameState {Players = players.ToArray()};
        }

        private void ProcessBroadcast()
        {
            var watch = new Stopwatch();

            while (true)
            {
                watch.Restart();

                _messenger.Broadcast(NetworkFactory.UdpBroadcastEndpoint);
                Console.WriteLine("Sent broadcast");

                var sleep = BroadcastDelay - (int) watch.ElapsedMilliseconds;
                if (sleep > 0)
                {
                    Thread.Sleep(sleep);
                }
            }
        }

        public void Dispose()
        {
            _network.Dispose();
        }

        public void Run()
        {
            Console.WriteLine("Started");
            _broadcastThread.Start();
            _sendThread.Start();
            _receiveThread.Start();

            _broadcastThread.Join();
        }
    }
}