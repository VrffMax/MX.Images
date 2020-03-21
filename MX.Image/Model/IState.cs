using System;
using System.Collections.Concurrent;

namespace MX.Images.Model
{
    public interface IState
    {
        public ConcurrentQueue<string> Messages { get; }

        public void Log(string source, Exception exception);
    }
}