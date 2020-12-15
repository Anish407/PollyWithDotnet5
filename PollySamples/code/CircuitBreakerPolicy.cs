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
        public CircuitBreakerPolicy<HttpResponseMessage> AdvancedBreakerPolicy { get; set; }
        public RetryPolicy<HttpResponseMessage> RetryPolicy { get; set; }

        public CircuitBreakerPoliciesWrapper()
        {
            BreakerPolicy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                             .CircuitBreakerAsync(2, TimeSpan.FromSeconds(5));

            AdvancedBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                             //1-> 0.5 = IF 50% of requests are  !response.IsSuccessStatusCode
                             //2-> TimeSpan.FromSeconds(60) = IF 50% OF REQ WITHIN 60 SEC
                             //3-> 7 -- IF MAX NO OF REQ WITHIN 60 SECS IS LESS THAN 7 THEN IGNORE POLICY
                             //4-> TimeSpan.FromSeconds(3) -- OPEN CIRCUIT FOR 3 SECS
                             .AdvancedCircuitBreakerAsync(0.5,TimeSpan.FromSeconds(60),7,TimeSpan.FromSeconds(3));

            RetryPolicy =Policy.HandleResult<HttpResponseMessage>(res=> !res.IsSuccessStatusCode)
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
