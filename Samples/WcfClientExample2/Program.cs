using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using TestService;
using WcfClientFactory;

namespace WcfClientExample2
{
    class Program
    {
        static void Main(string[] args)
        {
            Guid customHeaderToken = Guid.NewGuid();

            var testServiceClient = WcfClientProxyFactory.CreateInstance<ITestService, TestClientBase>(
                                                                                       TestServiceConstants.ServiceName,
                                                                                       TestServiceConstants.BaseAddress,
                                                                                       customHeaderToken);            
            Console.WriteLine("Sending token : {0} in the header",customHeaderToken);
            Console.WriteLine("6 + 7 = {0}", testServiceClient.Add(6, 7));
            Console.WriteLine("6 - 7 = {0}", testServiceClient.Substract(6, 7));
            Console.WriteLine(testServiceClient.Hello("World"));

            var testServiceClientWithCallback = WcfClientProxyFactory.CreateInstance<ITestService, TestClientBase>(
                                                                                (oc) => OnBeforeRequest(oc),
                                                                                (oc) => OnAfterRequest(oc),
                                                                                       TestServiceConstants.ServiceName,
                                                                                       TestServiceConstants.BaseAddress,
                                                                                       customHeaderToken);

            Console.WriteLine("Sending token : {0} in the header", customHeaderToken);
            Console.WriteLine("6 + 7 = {0}", testServiceClientWithCallback.Add(6, 7));
            Console.WriteLine("6 - 7 = {0}", testServiceClientWithCallback.Substract(6, 7));
            Console.WriteLine(testServiceClientWithCallback.Hello("World"));
        }

        public static void OnBeforeRequest(OperationContext operationContext)
        {
            Console.WriteLine("before request callback..");
        }

        public static void OnAfterRequest(OperationContext operationContext)
        {
            Console.WriteLine("after request callback..");
        }
    }
}
