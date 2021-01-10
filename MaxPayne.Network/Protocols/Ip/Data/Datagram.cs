using System.IO;

namespace MaxPayne.Network.Protocols.Ip.Data
{
    public readonly struct Datagram : IData
    {
        public Datagram(byte[] datagram)
        {
            Buffer = datagram;
        }

        public Datagram(FromStreamDelegate factory)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            factory(writer);
            Buffer = stream.ToArray();
        }

        public byte[] Buffer { get; }
        public int Length => Buffer.Length;
    }

    public delegate void FromStreamDelegate(BinaryWriter stream);
}
