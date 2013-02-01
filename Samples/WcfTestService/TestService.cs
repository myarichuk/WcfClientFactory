using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace TestService
{
    public class TestServiceImplementation : ITestService
    {
        public int Add(int x, int y)
        {
            DisplayCustomTokenFromHeaderIfExists();
            return x + y;
        }


        public int Substract(int x, int y)
        {
            DisplayCustomTokenFromHeaderIfExists();
            return x - y;
        }


        public string Hello(string target)
        {
            DisplayCustomTokenFromHeaderIfExists();
            return "Hello " + target;
        }

        private static void DisplayCustomTokenFromHeaderIfExists()
        {
            var customTokenHeader = OperationContext.Current
                                                    .IncomingMessageHeaders
                                                    .FirstOrDefault(h => h.Name == "customToken");
            
            if (customTokenHeader != null)
            {
                var customToken = OperationContext.Current
                                                  .IncomingMessageHeaders
                                                  .GetHeader<Guid>(customTokenHeader.Name, customTokenHeader.Namespace);
                Console.WriteLine(String.Format("Received custom token in header : {0}", customToken));
            }
        }
    }
}
