using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace PollySamples
{
    //CREATE POLICY REGISTRY(WHICH IS A CONCURRENT DICTIONARY)
    public class SharingPolicies
    {
        // create properties for each policy and then instantiate this class
        // or add it as a singleton/scoped in the DI container
        public RetryPolicy<HttpResponseMessage> RetryPolicy { get; private set; }

        public RetryPolicy RetryException { get; private set; }

        public SharingPolicies()
        {
            RetryPolicy = Policy
                   .HandleResult<HttpResponseMessage>(response => !response.IsSuccessStatusCode)
                    .RetryAsync(2, (response, retryCount) =>
                    {
                        if (response.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            //ReAuthorize
                        }

                    });

            //in case of an exception wait for timespan and retry 3 times
            RetryException = Policy.Handle<Exception>(ex => !string.IsNullOrWhiteSpace(ex.Message))
                            .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Pow(2, retryCount) / 2));
        }
    }
}
