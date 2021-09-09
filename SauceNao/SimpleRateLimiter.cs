using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace unbooru.SauceNao
{
    public class SimpleRateLimiter
    {
        private readonly ILogger<SimpleRateLimiter> _logger;

        private readonly object _lock = new();

        // No more than 4 every 30s
        private const int ShortDelay = 8200;

        private readonly Stopwatch _requestWatch = new();

        public SimpleRateLimiter(ILogger<SimpleRateLimiter> logger)
        {
            _logger = logger;
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
                    _logger.LogInformation("Time since last request is {Delay} ms, not throttling", delay);
                    _requestWatch.Restart();
                    return;
                }

                // add 50ms for good measure
                var waitTime = currentDelay - (int) delay;

                _logger.LogInformation("Time since last request is {Delay} ms, throttling for {WaitTime}", delay, waitTime);
                Thread.Sleep(waitTime);

                _logger.LogTrace("Finished Waiting. Sending request");
                _requestWatch.Restart();
            }
        }
    }
}
