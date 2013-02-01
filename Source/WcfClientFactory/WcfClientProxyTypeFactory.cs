using Fasterflect;
using FluentIL;
using FluentIL.Infos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel;
using FluentIL.Emitters;

namespace WcfClientFactory
{
    /// <summary>
    /// WCF Client Proxy creation factory
    /// </summary>
    /// <typeparam name="TServiceInterface">WCF service interface</typeparam>
    /// <typeparam name="TBaseClass">Base class for client channel. Must inherit from ClientBase`1</typeparam>
    internal class WcfClientProxyTypeFactory<TServiceInterface, TBaseClass>
        where TBaseClass : ClientBase<TServiceInterface>
        where TServiceInterface : class
    {
        #region Constants

        private const string SERVICE_CLIENT_CONSTRUCTOR_PARAMETERS_FIELD_NAME = "m_ServiceClientConstructorParameters";
        private const string BEFORE_OPERATION_CALLBACK_FIELD_NAME = "m_BeforeOperationCallback";
        private const string AFTER_OPERATION_CALLBACK_FIELD_NAME = "m_AfterOperationCallback";
        private const string CLIENT_PROXY_TYPE_NAME = "WcfServiceProxy";
        private const string METHOD_RESULT_VARIABLE_NAME = "methodResult";
        private const string OPERATION_CONTEXT_SCOPE_VARIABLE_NAME = "operationContextScope";
        private const string SERVICE_CLIENT_VARIABLE_NAME = "serviceClientProxy";
        public const string INIT_CALLBACKS_METHOD_NAME = "InitCallbacks";
        private const string OPERATION_CONTEXT_VARIABLE_NAME = "currentOperationContext";

        #endregion

        #region Members

        private readonly DynamicTypeInfo m_ClientProxyTypeInfo;
        private readonly Type m_ProxyClientChannelType;
        private IEnumerable<Object> m_ConstructorParameters;
        private readonly PropertyInfo m_ClientChannelPropertyInfo;

        #endregion

        #region Constructor(s)

        private WcfClientProxyTypeFactory()
        {
            m_ProxyClientChannelType = WcfClientBaseTypeFactory<TServiceInterface, TBaseClass>.GenerateType();
            m_ClientChannelPropertyInfo = m_ProxyClientChannelType.Property(WcfClientBaseTypeFactory<TServiceInterface, TBaseClass>.ClientChannelPropertyName, Flags.AllMembers);

            m_ClientProxyTypeInfo = IL.NewType(CLIENT_PROXY_TYPE_NAME)
                                      .Implements<TServiceInterface>()
                                      .WithField(SERVICE_CLIENT_CONSTRUCTOR_PARAMETERS_FIELD_NAME, typeof(object[]))
                                      .WithField(BEFORE_OPERATION_CALLBACK_FIELD_NAME, typeof(Action<OperationContext>))
                                      .WithField(AFTER_OPERATION_CALLBACK_FIELD_NAME, typeof(Action<OperationContext>))
                                      .WithMethod(INIT_CALLBACKS_METHOD_NAME, m => m.WithParameter(typeof(Action<OperationContext>))
                                                                                    .WithParameter(typeof(Action<OperationContext>))
                                                                                    .Returns(typeof(void))
                                                                                    .Ldarg(0)
                                                                                    .Ldarg(1)
                                                                                    .Stfld(BEFORE_OPERATION_CALLBACK_FIELD_NAME)
                                                                                    .Ldarg(0)
                                                                                    .Ldarg(2)
                                                                                    .Stfld(AFTER_OPERATION_CALLBACK_FIELD_NAME)
                                                                                    .Ret());

            m_ConstructorParameters = new object[] { };            
            ImplementConstructor(m_ClientProxyTypeInfo,new Type[0]); //implement empty constructor                     
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// generate factory with default settings
        /// </summary>
        internal static WcfClientProxyTypeFactory<TServiceInterface, TBaseClass> Create()
        {
            var newFactory = new WcfClientProxyTypeFactory<TServiceInterface, TBaseClass>();

            return newFactory;
        }

        /// <summary>
        /// generate client proxy type
        /// </summary>
        /// <remarks>after using this method no additional fluent methods can be called</remarks>
        internal Type GenerateType()
        {
            var serviceInterfaceMethods = typeof(TServiceInterface).Methods();
            m_ClientProxyTypeInfo.Repeater(serviceInterfaceMethods,
                                            (interfaceMethod, clientProxyTypeInfo) =>
                                                clientProxyTypeInfo.WithMethod(interfaceMethod.Name, m =>
                                                    ClientProxyMethodDefinition(m, interfaceMethod)));
            return m_ClientProxyTypeInfo.AsType;
        }

        /// <summary>
        /// set constructor parameters for client proxy type
        /// </summary>
        /// <param name="parameters">parameter list for client proxy - those values will be passed for 
        /// each proxy method used (when the client channel is created)</param>
        internal WcfClientProxyTypeFactory<TServiceInterface, TBaseClass> WithConstructorParameters(params object[] parameters)
        {
            m_ConstructorParameters = parameters.ToArray(); //do not hold reference to collection
            var parameterTypes = parameters.Select(x => x.GetType()).ToArray();
            var baseClassType = typeof(TBaseClass);
            if (baseClassType.Constructor(parameterTypes) == null)
            {
                throw new ApplicationException(String.Format("Constructor with specified parameters not defined on type {0}", baseClassType.Name));
            }

            ImplementConstructor(m_ClientProxyTypeInfo, parameterTypes);
            return this;

        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// retrieve most fitting constructor for specified constructor parameters
        /// </summary>
        private ConstructorInfo GetProxyClientConstructorInfo()
        {
            var selectedConstructor = m_ProxyClientChannelType.Constructor(m_ConstructorParameters.Select(x => x.GetType())
                                                                                                  .ToArray());
            if (selectedConstructor == null)
            {
                selectedConstructor = m_ProxyClientChannelType.Constructor(new Type[0]); //default constructor
            }

            return selectedConstructor;
        }

        private void ClientProxyMethodDefinition(DynamicMethodInfo clientProxyMethod, MethodInfo interfaceMethodToProxy)
        {
            var methodParameters = interfaceMethodToProxy.Parameters();
            //TODO : add exception handling instead of rethrow
            clientProxyMethod.WithParameters(methodParameters)
                             .WithVariable(m_ProxyClientChannelType, SERVICE_CLIENT_VARIABLE_NAME)
                             .WithVariable(interfaceMethodToProxy.ReturnType, METHOD_RESULT_VARIABLE_NAME)
                             .WithVariable(typeof(OperationContext),OPERATION_CONTEXT_VARIABLE_NAME)
                             .WithVariable(typeof(OperationContextScope),OPERATION_CONTEXT_SCOPE_VARIABLE_NAME)
                             .Returns(interfaceMethodToProxy.ReturnType)
                             .Try(body: m => ClientProxyMethodBody(m,interfaceMethodToProxy),
                                  catches:IL.Catch<Exception>(m => m.Throw()),
                                  @finally: m => m.Try(mc => mc.Ldloc(SERVICE_CLIENT_VARIABLE_NAME)
                                                             .Call<ClientBase<TServiceInterface>>("Close"),
                                                    IL.Catch<Exception>(mce => mce.Ldloc(SERVICE_CLIENT_VARIABLE_NAME)
                                                                              .Call<ClientBase<TServiceInterface>>("Abort")))
                                                  .Ldloc(OPERATION_CONTEXT_SCOPE_VARIABLE_NAME)
                                                  .Call<IDisposable>("Dispose")
                                                  
                                   )
                             .Ldloc(METHOD_RESULT_VARIABLE_NAME)
                             .Ret();
        }

        private void ClientProxyMethodBody(DynamicMethodBody methodBody, MethodInfo methodInfoToImplement)
        {
            var proxyClientClassConstructor = GetProxyClientConstructorInfo();
            var proxyClientClassConstructorParameters = proxyClientClassConstructor.Parameters().ToArray();
            var invokeMethod = typeof(Action<OperationContext>).Method("Invoke",BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var oprerationContextCurrentGetMethod = typeof(OperationContext).Method("get_Current", BindingFlags.Static | BindingFlags.Public);

            CreateClientProxy(methodBody,
                              proxyClientClassConstructor,
                              proxyClientClassConstructorParameters)
                    .Stloc(SERVICE_CLIENT_VARIABLE_NAME)
                    .Ldloc(SERVICE_CLIENT_VARIABLE_NAME)
                    .CallGet<ClientBase<TServiceInterface>>("InnerChannel")
                    .Newobj<OperationContextScope>(typeof(IContextChannel))
                    .Stloc(OPERATION_CONTEXT_SCOPE_VARIABLE_NAME)
                    .Call(oprerationContextCurrentGetMethod)
                    .Stloc(OPERATION_CONTEXT_VARIABLE_NAME)
                    .Ldarg(0)
                    .Ldfld(BEFORE_OPERATION_CALLBACK_FIELD_NAME)
                    .Ldloc(OPERATION_CONTEXT_VARIABLE_NAME)
                    .Callvirt(invokeMethod)
                    .Ldloc(SERVICE_CLIENT_VARIABLE_NAME)
                    .Callvirt(m_ClientChannelPropertyInfo.GetGetMethod())
                    .Repeater(1, methodInfoToImplement.Parameters().Count, 1,
                             (i, m) => m.Ldarg((uint)i))
                    .Callvirt(methodInfoToImplement)
                    .Stloc(METHOD_RESULT_VARIABLE_NAME)
                    .Ldarg(0)
                    .Ldfld(AFTER_OPERATION_CALLBACK_FIELD_NAME)
                    .Ldloc(OPERATION_CONTEXT_VARIABLE_NAME)
                    .Callvirt(invokeMethod);

        }

        private static DynamicMethodBody CreateClientProxy(
            DynamicMethodBody methodBody, 
            ConstructorInfo proxyClientClassConstructor, 
            ParameterInfo[] proxyClientClassConstructorParameters)
        {
            return methodBody.Repeater(0, proxyClientClassConstructorParameters.Length - 1, 1,
                                (i, repeaterMethodBody) => repeaterMethodBody
                                          .Ldarg(0)
                                          .Ldfld(SERVICE_CLIENT_CONSTRUCTOR_PARAMETERS_FIELD_NAME)
                                          .LdcI4(i)
                                          .Ldelem_Ref()
                                          .UnboxAny(proxyClientClassConstructorParameters[i].ParameterType))
                             .Newobj(proxyClientClassConstructor);
        }

        private static void ImplementConstructor(DynamicTypeInfo proxyTypeInfo, Type[] constructorArgs)
        {
            if (constructorArgs == null) return;
            if (!constructorArgs.Any())
            {
                proxyTypeInfo.WithConstructor(c => c.BodyDefinition()
                                                    .Ldarg(0)
                                                    .Newarr(typeof(object), 0)
                                                    .Stfld(SERVICE_CLIENT_CONSTRUCTOR_PARAMETERS_FIELD_NAME)
                                                    .Ret());
            }
            else
            {
                proxyTypeInfo.WithConstructor(c => c.BodyDefinition()
                                                    .Ldarg(0)
                                                    .Newarr(typeof(object),constructorArgs.Length)                                                    
                                                    .Stfld(SERVICE_CLIENT_CONSTRUCTOR_PARAMETERS_FIELD_NAME)
                                                    .Repeater(0, constructorArgs.Length - 1, 1, (i, m) =>
                                                        m.Ldarg(0)
                                                         .Ldfld(SERVICE_CLIENT_CONSTRUCTOR_PARAMETERS_FIELD_NAME)
                                                         .LdcI4(i) //load array index to stack
                                                         .Ldarg((uint)i + 1)
                                                         .Box(constructorArgs[i])
                                                         .Stelem_Ref())
                                                    .Ret(),
                                              constructorArgs);
            }
        }
        #endregion
    }
}
