using Polly;
using Polly.Retry;
using Serilog;
using Serilog.Core;

namespace Caching.Core
{
    public class PollyRetry
    {
        //Create a service for Polly retry and implement the generci solution for both httpclient and s3client
        public async static Task<TResult> ExecuteAsync<TResult>(
        Func<Task<TResult>> actionFunc,
        Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>> fallback,
        Func<DelegateResult<TResult>, Context, Task> onFallbackAsync, ILogger logger)
        {
            var fallbackPolicy = Policy<TResult>.Handle<Exception>()
                                    .FallbackAsync(fallback, onFallbackAsync);

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                   (ex, timeSpan, retryCount, context) =>
                   {
                       // Log exception
                       logger.Error(ex, "Retry attempt: {RetryCount} failed. Retrying in {Time} seconds.", retryCount, timeSpan);
                   });
            var policyResult = await fallbackPolicy.WrapAsync(retryPolicy).ExecuteAndCaptureAsync(actionFunc);
            return policyResult.Result;
        }

        public static RetryPolicy GetRetryPolicy(ILogger logger)
        {
            return Policy
                .Handle<Exception>() // You can customize this to handle specific exceptions if needed
                .WaitAndRetry(
                    3,
                    retryAttempt =>
                    {
                        logger.Error($"Retrying connection attempt {retryAttempt}");
                        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    },
                    (exception, timeSpan, retryCount, context) =>
                    {
                        logger.Error(exception, $"Error connecting to Redis. RetryCount: {retryCount}, Delay: {timeSpan.TotalSeconds} seconds");
                    });
        }

        public static TResult Execute<TResult>(
        Func<TResult> actionFunc,
        Func<DelegateResult<TResult>, Context, CancellationToken, TResult> fallback,
        Action<DelegateResult<TResult>, Context> onFallback, ILogger logger)
        {
            {
                var fallbackPolicy = Policy<TResult>.Handle<Exception>()
                                    .Fallback<TResult>(fallback, onFallback);

                var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, timeSpan, retryCount, context) =>
                    {
                        // Log exception
                        logger.Error(ex, "Retry attempt: {RetryCount} failed. Retrying in {Time} seconds.", retryCount, timeSpan);
                    });

                var policyResult = fallbackPolicy.Wrap(retryPolicy).ExecuteAndCapture(actionFunc);
                return policyResult.Result;
            }
        }
    }
}