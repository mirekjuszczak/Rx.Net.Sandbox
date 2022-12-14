using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Reactive.Net.Sandbox.ConsoleObserver;

namespace Reactive.Net.Sandbox.Samples
{
    public class FromEventPattern
    {
        public static void RunSample()
        {
            var eventBroadcaster = new EventBroadcaster();

            Observable.FromEventPattern<int>(
                    handler => eventBroadcaster.RandomNumber += handler,
                    handler => eventBroadcaster.RandomNumber -= handler)
                .Select(h=> h.EventArgs)
                .Do(Print)
                .SubscribeConsole("value get from event");
        }
        
        private static void Print(int number)
        {
            Console.Write($">>> {number} <<<");
        }
    }

    public class EventBroadcaster
    {
        public event EventHandler<int>? RandomNumber;

        public EventBroadcaster()
        {
            Task.Run(async () =>
            {
                var rnd = new Random();
                while (true)
                {
                    await Task.Delay(rnd.Next(500, 1000));
                    RandomNumber?.Invoke(this, rnd.Next(100,200));
                }
            });
        }
    }
}