using System.Collections.Concurrent;

namespace MX.Images.Models
{
    public class State
        : IState
    {
        public State()
        {
            Errors = new ConcurrentQueue<string>();
        }

        public ConcurrentQueue<string> Errors { get; }
    }
}