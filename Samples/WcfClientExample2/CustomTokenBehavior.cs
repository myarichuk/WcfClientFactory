using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace WcfClientExample2
{
    /// <summary>
    /// behavior that adds before request callback to client
    /// </summary>
    /// <remarks>
    /// many thanks to IDesign's example for the idea of using endpoint behavior
    /// and client message inspector together as one class
    /// </remarks>
    public class CustomTokenBehavior : IEndpointBehavior, IClientMessageInspector 
    {
        private readonly TestClientBase m_TestClientBase;

        public CustomTokenBehavior(TestClientBase testClientBase)
        {
            m_TestClientBase = testClientBase;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            m_TestClientBase.OnBeforeRequest(ref request);
            return null;
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        #region Unused Method Implementations

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion
    }
}
