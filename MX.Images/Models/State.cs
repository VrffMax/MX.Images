using System;
using System.Collections.Concurrent;

namespace MX.Images.Models
{
    public class State
        : IState
    {
        private const string ErrorPrefix = "*** Error ***";

        public State()
        {
            Messages = new ConcurrentQueue<string>();
        }

        public ConcurrentQueue<string> Messages { get; }

        public void Log(string source, Exception exception)
        {
            var text = $"{ErrorPrefix} {source} {exception.Message}";

            Messages.Enqueue(text);
            Console.WriteLine(text);
        }
    }
}