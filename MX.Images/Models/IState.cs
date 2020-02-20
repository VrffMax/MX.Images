using System.Collections.Concurrent;

namespace MX.Images.Models
{
    public interface IState
    {
        public ConcurrentQueue<string> Errors { get; }
    }
}