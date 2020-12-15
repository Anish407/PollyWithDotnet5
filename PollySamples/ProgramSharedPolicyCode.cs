using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PollySamples
{
    public class ProgramSharedPolicyCode
    {
        public static async Task Main(string[] args)
        {
            //CREATE POLICY REGISTRY(WHICH IS A CONCURRENT DICTIONARY) --PENDING
            Console.WriteLine("Starting");
            string exit = string.Empty;
            var provider = new ServiceCollection().AddSingleton(new SharingPolicies()).BuildServiceProvider();

            do
            {
                var sharingPolicies = provider.GetRequiredService<SharingPolicies>();

                await MakeHttpCall(async ()=> 
                     await sharingPolicies.RetryException.ExecuteAsync(
                     async() => await new HttpClient().GetAsync("https://localhost:44319/Demo")));

                Console.WriteLine("Try again? press x to exit");
                exit = Console.ReadLine();
            } while (exit != "x");

            Console.ReadLine();
        }
        static async Task MakeHttpCall(Func<Task<HttpResponseMessage>> operation)
        {
            var response = await operation();
            var data = await response.Content.ReadAsStringAsync();
            Console.WriteLine(data);
        }

    }

    
}
