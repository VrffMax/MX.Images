using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MX.Images.Models
{
    public interface IState
    {
        public ConcurrentQueue<string> Errors { get; }
    }
}