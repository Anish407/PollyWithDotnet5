using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollySamples.code
{
    public class CircuitBreakerPoliciesWrapper
    {
        public CircuitBreakerPolicy<HttpResponseMessage> BreakerPolicy { get; set; }
        public RetryPolicy<HttpResponseMessage> RetryPolicy { get; set; }

        public CircuitBreakerPoliciesWrapper()
        {
            BreakerPolicy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                             .CircuitBreakerAsync(2, TimeSpan.FromSeconds(5));

            RetryPolicy=Policy.HandleResult<HttpResponseMessage>(res=> !res.IsSuccessStatusCode)
                       .Or<BrokenCircuitException>()
                       .RetryAsync(25, async (response, retryCount) =>
                       {
                           await Task.Delay(TimeSpan.FromSeconds(2));
                           if (response.Result !=null)
                           {
                               Console.WriteLine($"Inside Retry: {DateTime.Now.TimeOfDay}, RetryCount: {retryCount}");
                               Console.WriteLine(await response?.Result?.Content?.ReadAsStringAsync());
                           }
                           else
                           {
                               Console.WriteLine(response.Exception.Message);
                           }
                           //string msg = await response?.Result?.Content?.ReadAsStringAsync() ?? "hello";
                           //Console.WriteLine(msg);
                       });
        }
    }
}
