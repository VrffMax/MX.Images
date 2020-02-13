using System.Collections.Concurrent;

namespace MX.Images.Models
{
    public class State
        : IState
    {
        public State() =>
            Messages = new ConcurrentQueue<string>();

        public ConcurrentQueue<string> Messages { get; }
    }
}