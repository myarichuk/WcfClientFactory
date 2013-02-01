using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using TestService;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace WcfClientExample2
{    
    public class TestClientBase : ClientBase<ITestService>, IDisposable
    {
        private readonly Guid m_CustomToken;
        public TestClientBase(string endpointConfigurationName, string remoteAddress,Guid customToken)
            : base(endpointConfigurationName, remoteAddress)
        {
            m_CustomToken = customToken;
            Endpoint.Behaviors.Add(new CustomTokenBehavior(this));
        }

        public void OnBeforeRequest(ref Message requestMessage)
        {
            var customHeader = new MessageHeader<Guid>(m_CustomToken);
            var customUntypedHeader = customHeader.GetUntypedHeader("customToken", "http://customToken");
            requestMessage.Headers.Add(customUntypedHeader);
        }
    }
}
