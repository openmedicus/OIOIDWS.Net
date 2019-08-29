using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Digst.OioIdws.Common.Logging;
using Digst.OioIdws.HelloService;
using log4net;

namespace Digst.OioIdws.WspHealthcareExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create the ServiceHost.
            using (var host = new ServiceHost(typeof(HelloWorld)))
            {
                host.Open();

                // Ensure WSP only uses TLS 1.2 to communicate with WSC.
                // 
                // Note: As this can't be enforced by code/configuration, you
                // MUST use a tool like "IIS Crypto" (free) where you can choose 
                // the PCI 3.1 Template and unmark TLS 1.1) to enforce this on
                // an Operating System level.
                // 
                // Source: https://www.nartac.com/Products/IISCrypto

                Console.WriteLine($"The {host.Description.Name} service is ready.");

                foreach (var endpoint in host.Description.Endpoints)
                {
                    Console.WriteLine($"listening using {endpoint.Binding.Name} at {endpoint.Address.Uri}");
                }

                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }
        }
    }


}