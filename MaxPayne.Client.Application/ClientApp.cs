using System.Diagnostics;
using MaxPayne.Client.Backend;
using MaxPayne.Messages.State;

namespace MaxPayne.Client.Application
{
    public static class ClientApp
    {
        private static IBackend? _backend;

        public static void OnAttach()
        {
            _backend ??= new Backend.Backend();
            _backend.Start();
        }

        public static FrameState Frame = new FrameState();
        public static GameState OnFrame()
        {
            Debug.Assert(_backend is not null);
            return _backend.Frame(Frame);
        }

        public static GameState OnFrame(FrameState state)
        {
            Debug.Assert(_backend is not null);
            return _backend.Frame(state);
        }

        public static void OnDetach()
        {
            _backend?.Dispose();
        }
    }
}