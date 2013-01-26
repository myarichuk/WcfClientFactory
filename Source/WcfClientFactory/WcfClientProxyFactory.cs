using Fasterflect;
using System;
using System.Linq;
using System.ServiceModel;

namespace WcfClientFactory
{
    public static class WcfClientProxyFactory
    {
        /// <summary>
        /// Create WCF service proxy client.
        /// </summary>
        /// <typeparam name="TServiceInterface">service contract interface</typeparam>
        /// <param name="constructorParams">constructor parameters for  ClientBase`1 -&gt; </param>
        /// <returns>wcf proxy client with channel class inherited from ClientBase`1</returns>
        public static TServiceInterface CreateInstance<TServiceInterface>(params object[] constructorParams)
            where TServiceInterface : class
        {
            return CreateInstance<TServiceInterface, ClientBase<TServiceInterface>>(constructorParams);
        }

        /// <summary>
        /// Create WCF service proxy client
        /// </summary>
        /// <typeparam name="TServiceInterface">service contract interface</typeparam>
        /// <typeparam name="TBaseClass">channel base class that inherits from ClientBase</typeparam>
        /// <param name="constructorParams">constructor parameters for TBaseClass</param>
        /// <returns>wcf proxy client with channel class inherited from <see cref="TBaseClass">TBaseClass</see></returns>
        public static TServiceInterface CreateInstance<TServiceInterface, TBaseClass>(params object[] constructorParams)
            where TBaseClass : ClientBase<TServiceInterface>
            where TServiceInterface : class
        {
            if (!typeof(TServiceInterface).Attributes(typeof(ServiceContractAttribute)).Any())
            {
                throw new ArgumentException("Provided service interface must have 'ServiceContract' attribute. Cannot create service proxy.");
            }

            var clientChannelType = WcfClientBaseTypeFactory<TServiceInterface, TBaseClass>.GenerateType();
            var proxyClientType = WcfClientProxyTypeFactory<TServiceInterface, TBaseClass>
                                                    .Create()
                                                    .WithConstructorParameters(constructorParams)
                                                    .GenerateType();
            
            return (TServiceInterface)proxyClientType.CreateInstance(constructorParams);            
        }
    }
}
