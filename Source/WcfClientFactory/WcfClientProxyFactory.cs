using Fasterflect;
using System;
using System.Linq;
using System.ServiceModel;

namespace WcfClientFactory
{
    /// <summary>
    /// proxy factory that emits at runtime WCF client proxies
    /// </summary>
    public static class WcfClientProxyFactory
    {
        private readonly static Action<OperationContext> EmptyCallback = (oc) => { };

        /// <summary>
        /// Create WCF service proxy client.
        /// </summary>
        /// <typeparam name="TServiceInterface">service contract interface</typeparam>
        /// <param name="constructorParams">constructor parameters for  ClientBase`1 -&gt; </param>
        /// <returns>wcf proxy client with channel class inherited from ClientBase`1</returns>
        public static TServiceInterface CreateInstance<TServiceInterface>(params object[] constructorParams)
            where TServiceInterface : class
        {
            return CreateInstance<TServiceInterface, ClientBase<TServiceInterface>>(EmptyCallback,EmptyCallback,constructorParams);
        }

        /// <summary>
        /// Create WCF service proxy client.
        /// </summary>
        /// <typeparam name="TServiceInterface">service contract interface</typeparam>
        /// <param name="beforeOperationCallback">delegate that will be executed before each operation</param>
        /// <param name="afterOperationCallback">delegate that will be executed after each operation</param>
        /// <param name="constructorParams">constructor parameters for  ClientBase`1 -&gt; </param>
        /// <returns></returns>
        public static TServiceInterface CreateInstance<TServiceInterface>(
            Action<OperationContext> beforeOperationCallback,
            Action<OperationContext> afterOperationCallback,
            params object[] constructorParams)
            where TServiceInterface : class
        {
            return CreateInstance<TServiceInterface, ClientBase<TServiceInterface>>(beforeOperationCallback,afterOperationCallback,constructorParams);
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
            return CreateInstance<TServiceInterface, TBaseClass>(EmptyCallback,EmptyCallback, constructorParams);
        }

        /// <summary>
        /// Create WCF service proxy client
        /// </summary>
        /// <typeparam name="TServiceInterface">service contract interface</typeparam>
        /// <typeparam name="TBaseClass">channel base class that inherits from ClientBase</typeparam>
        /// <param name="beforeOperationCallback">delegate that will be executed before each operation</param>
        /// <param name="afterOperationCallback">delegate that will be executed after each operation</param>
        /// <param name="constructorParams">constructor parameters for TBaseClass</param>
        /// <returns>wcf proxy client with channel class inherited from <see cref="TBaseClass">TBaseClass</see></returns>
        public static TServiceInterface CreateInstance<TServiceInterface, TBaseClass>(
                    Action<OperationContext> beforeOperationCallback,
                    Action<OperationContext> afterOperationCallback,
                    params object[] constructorParams)
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
                        
            var clientInstance = (TServiceInterface)Activator.CreateInstance(proxyClientType, constructorParams);

            var initCallbacksMethod = proxyClientType.Method(WcfClientProxyTypeFactory<TServiceInterface, TBaseClass>.INIT_CALLBACKS_METHOD_NAME);
            if (initCallbacksMethod != null)
            {
                initCallbacksMethod.Invoke(clientInstance, new[] { beforeOperationCallback, afterOperationCallback });
            }

            return clientInstance;
        }
    }
}
