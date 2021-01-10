namespace MaxPayne.Network
{
    public interface IData
    {
        byte[] Buffer { get; }
        int Length { get; }
    }
}