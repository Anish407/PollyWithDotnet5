using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Wrap;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace PollySamples.code
{
    public class WrapPolicies
    {
        public WrapPolicies()
        {
           
        }


        public async Task CallPolicyWrap()
        {
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(1);

            //handle defines the type of exception this policy can handle
            var exceptionPolicy = Policy.Handle<Exception>();

            RetryPolicy<HttpResponseMessage> retryPolicy =
               Policy.HandleResult<HttpResponseMessage>((response) => !response.IsSuccessStatusCode)
               .RetryAsync(2, (response, retryCount) =>
               {
                   if (response.Result.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                   {
                       //Log
                   }
               });

            FallbackPolicy<HttpResponseMessage> fallbackPolicy =
                Policy.HandleResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.InternalServerError)
                      .FallbackAsync(new HttpResponseMessage(statusCode: HttpStatusCode.OK)
                      {
                          Content = new ObjectContent(typeof(string), "", new JsonMediaTypeFormatter())
                      });

            var wrap = PolicyWrap.WrapAsync(fallbackPolicy, retryPolicy, timeoutPolicy);

            var client = new HttpClient();
            var response = await wrap.ExecuteAsync(async ()=>await client.GetAsync("https://localhost:44319/Demo"));
            Console.WriteLine(await response.Content.ReadAsStringAsync()); 
           
        }
    }
}
