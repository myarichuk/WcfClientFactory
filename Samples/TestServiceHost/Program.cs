using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using TestService;

namespace TestServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {

            Uri baseAddress = new Uri(TestServiceConstants.BaseAddress);

            // Create the ServiceHost.
            using (ServiceHost host = new ServiceHost(typeof(TestServiceImplementation), baseAddress))
            {
                WSHttpBinding endpoint1Binding = new WSHttpBinding(SecurityMode.None, false);
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior() { HttpGetEnabled = true };
                host.Description.Behaviors.Add(smb);
                host.AddServiceEndpoint(typeof(ITestService), endpoint1Binding, "/");
                host.Open();

                Console.WriteLine("Press any key for closing the service..");
                Console.ReadLine();
            }
        }
    }
}
