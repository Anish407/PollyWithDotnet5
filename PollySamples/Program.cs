using Polly;
using Polly.Fallback;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace PollySamples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting");
            string exit = string.Empty;

            do
            {
                await RetryWithWaitPolly();
                Console.WriteLine("Try again? press x to exit");
                exit = Console.ReadLine();
            } while (exit != "x");

            Console.ReadLine();
        }

        static async Task RetryWithFallback()
        {
            RetryPolicy<HttpResponseMessage> retryPolicy = Policy
                                                            .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                                                            .RetryAsync(3);
            
            //If an 500 status code is returned then we return dummy or cached data
            FallbackPolicy<HttpResponseMessage> fallbackPolicy = 
                            Policy.HandleResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.InternalServerError)
                                  .FallbackAsync(new HttpResponseMessage(statusCode: HttpStatusCode.OK)
                                  {
                                      Content = new ObjectContent(typeof(string), "", new JsonMediaTypeFormatter())
                                  });
           
            await CallEndpoint(retryPolicy, fallbackPolicy);
        }

        static async Task<string> WithoutPolly()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://localhost:44319/Demo");
            return await response.Content.ReadAsStringAsync();
        }

        static async Task RetryWithPolly()
        {
            RetryPolicy<HttpResponseMessage> retryPolicy =
                Policy.HandleResult<HttpResponseMessage>((response) => !response.IsSuccessStatusCode)
                .RetryAsync(2);

            await CallEndpoint(retryPolicy);
        }


        static async Task RetryWithPollyWithCondition()
        {
            RetryPolicy<HttpResponseMessage> retryPolicy =
                Policy.HandleResult<HttpResponseMessage>((response) => !response.IsSuccessStatusCode)
                .Or<TimeoutException>() //handle timeout exception
                .RetryAsync(2, (response, retryCount) =>
                {
                    if (response.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        //ReAuthorize
                    }

                });

            await CallEndpoint(retryPolicy);
        }

        static async Task RetryWithWaitPolly()
        {
            //retry with exponential backoff. Better to add jitter as all services might have the same retry logic
            RetryPolicy<HttpResponseMessage> retryPolicy =
                Policy.HandleResult<HttpResponseMessage>((response) => !response.IsSuccessStatusCode)
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2));

            await CallEndpoint(retryPolicy);
        }


        static async Task CallEndpoint(RetryPolicy<HttpResponseMessage> retryPolicy)
            => await MakeHttpCall(async () => await retryPolicy.ExecuteAsync(() => new HttpClient().GetAsync("https://localhost:44319/Demo")));

        static async Task CallEndpoint(RetryPolicy<HttpResponseMessage> retryPolicy, FallbackPolicy<HttpResponseMessage> fallbackPolicy)
            =>await MakeHttpCall(
                      () =>   fallbackPolicy.ExecuteAsync(
                      () =>  retryPolicy.ExecuteAsync(
                      () =>  new HttpClient().GetAsync("https://localhost:44319/Demo"))));

        static async Task MakeHttpCall(Func<Task<HttpResponseMessage>> operation)
        {
            var response = await operation();
            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine(data);
        }
    }
}
