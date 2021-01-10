using System;
using System.IO;

namespace MaxPayne.Messages.State
{
    public struct FrameState
    {
        public float X;
        public float Y;
        public float Z;
        public float RotationAngle;
        public float VerticalAngle;
        public byte Gun;
        public short Ammo1;
        public short Ammo2;
        public short Ammo3;

        public FrameState(BinaryReader stream)
        {
            X = stream.ReadSingle();
            Y = stream.ReadSingle();
            Z = stream.ReadSingle();
            RotationAngle = stream.ReadSingle();
            VerticalAngle = stream.ReadSingle();
            Gun = stream.ReadByte();
            Ammo1 = stream.ReadInt16();
            Ammo2 = stream.ReadInt16();
            Ammo3 = stream.ReadInt16();
        }

        public void Export(BinaryWriter stream)
        {
            stream.Write(X);
            stream.Write(Y);
            stream.Write(Z);
            stream.Write(RotationAngle);
            stream.Write(VerticalAngle);
            stream.Write(Gun);
            stream.Write(Ammo1);
            stream.Write(Ammo2);
            stream.Write(Ammo3);
        }
    }
}