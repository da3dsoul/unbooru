using System.Diagnostics;
using System.Threading;

namespace Meowtrix.PixivApi
{
    public class SimpleRateLimiter
    {

        private readonly object _lock = new();

        // No more than 4 every 30s
        private const int ShortDelay = 3500;

        private readonly Stopwatch _requestWatch = new();

        public SimpleRateLimiter()
        {
            _requestWatch.Start();
        }

        public void EnsureRate()
        {
            lock (_lock)
            {
                var delay = _requestWatch.ElapsedMilliseconds;

                var currentDelay = ShortDelay;

                if (delay > currentDelay)
                {
                    _requestWatch.Restart();
                    return;
                }

                // add 50ms for good measure
                var waitTime = currentDelay - (int) delay;
                Thread.Sleep(waitTime);
                _requestWatch.Restart();
            }
        }
    }
}
