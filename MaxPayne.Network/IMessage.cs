namespace MaxPayne.Network
{
    public interface IMessage<TEndpoint>
        where TEndpoint : struct
    {
        IData Data { get; }
        TEndpoint Endpoint { get; }
    }
}