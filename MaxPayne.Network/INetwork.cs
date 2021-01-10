using System;
using System.Collections.Generic;

namespace MaxPayne.Network
{
    public interface INetwork<TEndpoint> : IDisposable
        where TEndpoint : struct
    {
        void Send(IMessage<TEndpoint> message);
        IEnumerable<IMessage<TEndpoint>> ReceiveAll();
    }
}
