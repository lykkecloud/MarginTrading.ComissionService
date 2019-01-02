using System;
using Polly;
using Polly.Retry;

namespace MarginTrading.CommissionService.Core
{
    public static class ApiHelpers
    {
        /// <summary>
        /// Policy to call API method a number of times with retry awaiting period.
        /// It ignore Refit ApiException and awaits for return value to match <paramref name="resultPredicate"/>.
        /// </summary>
        /// <param name="resultPredicate">Predicate for API return message. Not used if null.</param>
        /// <param name="apiCallRetries"></param>
        /// <param name="apiCallRetryPeriodMs"></param>
        /// <typeparam name="T">API return message type</typeparam>
        public static RetryPolicy<T> RefitRetryPolicy<T>(Func<T, bool> resultPredicate,
            int apiCallRetries, int apiCallRetryPeriodMs)
        {
            return Policy.Handle<Refit.ApiException>()
                .OrResult<T>(resultPredicate)
                .WaitAndRetryAsync(apiCallRetries, x => TimeSpan.FromMilliseconds(apiCallRetryPeriodMs));
        }
    }
}