using System;
using MaxPayne.Messages.State;

namespace MaxPayne.Client.Backend
{
    public interface IBackend : IDisposable
    {
        void Start();
        GameState Frame(FrameState state);
    }
}