using PollySamples.code;
using System;
using System.Threading.Tasks;

namespace PollySamples
{
    public  class ProgramPolicyWrap
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting");
            string exit = string.Empty;

            do
            {
                await new WrapPolicies().CallPolicyWrap();
                Console.WriteLine("Try again? press x to exit");
                exit = Console.ReadLine();
            } while (exit != "x");

            Console.ReadLine();
        }
    }
}
