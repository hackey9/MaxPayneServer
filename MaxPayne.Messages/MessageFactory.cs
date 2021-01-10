using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MaxPayne.Messages.FromClient;
using MaxPayne.Messages.FromServer;
using MaxPayne.Network;
using MaxPayne.Network.Protocols.Ip;

namespace MaxPayne.Messages
{
    public static class MessageFactory
    {
        public static bool TryFactory(IMessage<IpEndpoint> datagram, [MaybeNullWhen(false)]out IPayload message)
        {
            var from = datagram.Endpoint;

            var stream = new MemoryStream(datagram.Data.Buffer, false);
            var reader = new BinaryReader(stream);

            var type = (MessageTypes)reader.ReadByte();

            try
            {
                message = type switch
                {
                    MessageTypes.BroadcastServerIsHere => new BroadcastServerIsHere(from),
                    MessageTypes.ClientWantsConnectToServer => new ClientWantsConnect(from),
                    MessageTypes.ClientConnectedToServer => new ClientConnected(from, reader),
                    MessageTypes.GetTheFuckOut => new GetOutOfHere(from, reader),
                    MessageTypes.ClientSentFrameState => new ClientSentFrame(from, reader),
                    MessageTypes.ServerSendGameState => new ServerSentGameState(from, reader),
                    _ => throw new InvalidEnumArgumentException(),
                };

                if (stream.Position != datagram.Data.Length)
                {
                    throw new Exception("Stream is not on the end");
                }
            }
            catch (Exception e)
            {
                message = null;
                Debugger.Break();
                Debug.WriteLine($"Catch {e.GetType()}: {e.Message}");
            }

            return message is not null;
        }
    }
}
