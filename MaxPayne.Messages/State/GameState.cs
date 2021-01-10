using System;
using System.Diagnostics;
using System.IO;

namespace MaxPayne.Messages.State
{
    public struct GameState
    {
        public PlayerState[] Players;

        public unsafe GameState(BinaryReader stream)
        {
            var count = stream.ReadByte();
            Players = new PlayerState[count];
            for (var i = 0; i < count; i++)
            {
                fixed (PlayerState* ptr = &Players[i])
                {
                    ptr->Id = stream.ReadInt32();
                    ptr->X = stream.ReadSingle();
                    ptr->Y = stream.ReadSingle();
                    ptr->Z = stream.ReadSingle();
                    ptr->RotationAngle = stream.ReadSingle();
                    ptr->VerticalAngle = stream.ReadSingle();
                    ptr->Gun = stream.ReadByte();
                }
            }
        }

        public unsafe void Export(BinaryWriter stream)
        {
            if (Players is null)
            {
                stream.Write((int) 0);
                return;
            }

            var length = (byte) Players.Length;

            stream.Write(length);

            for (var i = 0; i < length; i++)
            {
                fixed (PlayerState* ptr = &Players[i])
                {
                    stream.Write(ptr->Id);
                    stream.Write(ptr->X);
                    stream.Write(ptr->Y);
                    stream.Write(ptr->Z);
                    stream.Write(ptr->RotationAngle);
                    stream.Write(ptr->VerticalAngle);
                    stream.Write(ptr->Gun);
                }
            }
        }
    }

    public struct PlayerState
    {
        public int Id;
        public float X;
        public float Y;
        public float Z;
        public float RotationAngle;
        public float VerticalAngle;
        public byte Gun;
    }
}