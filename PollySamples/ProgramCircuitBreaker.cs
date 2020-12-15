using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;
using PollySamples.code;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollySamples
{
    public class ProgramCircuitBreaker
    {
        public static async Task Main(string[] arg)
        {
            // we register it as a singleton so that every httpclient uses the same policy instance
            // this way we can break the circuit for all the calls that use the same policy 
            // We can register it in different scope in the DI container and acheive different behavior 

            var serviceProvider = new ServiceCollection()
                                      .AddSingleton(new CircuitBreakerPoliciesWrapper())
                                      .BuildServiceProvider();
            var circuitBreaker = serviceProvider.GetRequiredService<CircuitBreakerPoliciesWrapper>();
            Console.WriteLine("Starting");
            string exit = string.Empty;
            do
            {
                //Console.WriteLine(await circuitBreaker.RetryPolicy.ExecuteAsync(
                //    () => new HttpClient().GetAsync("https://localhost:44319/Demo")
                //     )); 


                //var tasks = new List<Task>();

                // we add 2 tasks, the circuit breaker policy breaks the circuit after 
                //we get 2 or more !IsSuccessStatusCode for 25 secs. So the second call will fail as the 
                // circuit will be open after unsuccessfull retries, since both httpclients use the same
                //instance of the policy
                Console.WriteLine($"Start: {DateTime.Now.TimeOfDay}");
                Console.WriteLine(await circuitBreaker.RetryPolicy.ExecuteAsync(
                     () => circuitBreaker.BreakerPolicy.ExecuteAsync(
                     () => new HttpClient().GetAsync("https://localhost:44319/Demo"))));
                Console.WriteLine($"Done: {DateTime.Now.TimeOfDay}");
                //await Task.Delay(TimeSpan.FromSeconds(5));

                //Console.WriteLine(await circuitBreaker.RetryPolicy.ExecuteAsync(
                //     () => circuitBreaker.BreakerPolicy.ExecuteAsync(
                //     () => new HttpClient().GetAsync("https://localhost:44319/Demo"))));


                // await Task.WhenAll(tasks);

            } while (exit.Equals("x"));

        }
    }
}
