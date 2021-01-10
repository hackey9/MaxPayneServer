namespace MaxPayne.Messages
{
    public enum MessageTypes : byte
    {
        BroadcastServerIsHere,

        ClientWantsConnectToServer,
        ClientConnectedToServer,
        GetTheFuckOut,

        ClientSentFrameState,
        ServerSendGameState,
    }
}
