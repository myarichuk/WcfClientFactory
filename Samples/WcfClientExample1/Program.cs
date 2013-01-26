using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestService;
using WcfClientFactory;
namespace WcfClientExample1
{
    class Program
    {
        static void Main(string[] args)
        {
            var testServiceClient = WcfClientProxyFactory.CreateInstance<ITestService>(TestServiceConstants.ServiceName,
                                                                                       TestServiceConstants.BaseAddress);

            Console.WriteLine("6 + 7 = {0}", testServiceClient.Add(6, 7));
            Console.WriteLine("6 - 7 = {0}", testServiceClient.Substract(6, 7));
            Console.WriteLine(testServiceClient.Hello("World"));
        }
    }
}
