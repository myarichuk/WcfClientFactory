using Fasterflect;
using FluentIL;
using FluentIL.Infos;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel;

namespace WcfClientFactory
{
    /// <summary>
    /// helper class that generates ClientBase inherited type for opening/closing communication channel
    /// </summary>
    /// <typeparam name="TServiceInterface">service contract interface</typeparam>
    /// <typeparam name="TBaseClass">channel base class that inherits from ClientBase</typeparam>
    internal class WcfClientBaseTypeFactory<TServiceInterface, TBaseClass>
        where TBaseClass : ClientBase<TServiceInterface>
        where TServiceInterface : class
    {
        #region Private Members

        private const string CLIENT_CHANNEL_PROPERTY_NAME = "ClientChannel";
        private readonly Type m_ClientChannelType;
        
        #endregion

        #region Constructor(s)

        /// <summary>
        /// initiate the new type and implement ClientBase`1 constructors
        /// </summary>
        private WcfClientBaseTypeFactory() 
        {
            var baseClassType = typeof(TBaseClass);
            var getChannelMethod = baseClassType.Property("Channel")
                                                .GetAccessors(true)
                                                .First(m => m.Name.Contains("get"));

            var clientChannelType = IL.NewType(baseClassType.Name)
                                      .Inherits<TBaseClass>()
                                      .WithProperty(
                                          propertyName: CLIENT_CHANNEL_PROPERTY_NAME,
                                          propertyType: typeof(TServiceInterface),
                                          getmethod: m => m
                                              .Ldarg(0)
                                              .Callvirt(getChannelMethod)
                                              .Ret())
                                      .Repeater(typeof(TBaseClass).Constructors(),
                                            (constructorInfo, clientType) => ImplementConstructor(constructorInfo, clientType));

            m_ClientChannelType = clientChannelType.AsType;

        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// name of "Channel" property of ClientBase`1 inherited class
        /// </summary>
        internal static string ClientChannelPropertyName
        {
            get
            {
                return CLIENT_CHANNEL_PROPERTY_NAME;
            }
        }

        /// <summary>
        /// generate the client channel class type
        /// </summary>
        internal static Type GenerateType()
        {
            return (new WcfClientBaseTypeFactory<TServiceInterface, TBaseClass>()).m_ClientChannelType;
        }

        #endregion

        #region Helper Methods

        private static void ImplementConstructor(ConstructorInfo constructorInfo, DynamicTypeInfo clientChannelType)
        {
            clientChannelType.WithConstructor(c => c.BodyDefinition()
                                                    .Ldarg(0)
                                                    .Repeater(1, constructorInfo.Parameters().Count, 1, (i, m) => m.Ldarg((uint)i))
                                                    .Call(constructorInfo)
                                                    .Ret(),
                                              constructorInfo.Parameters()
                                                             .Select(x => x.ParameterType)
                                                             .ToArray());
        }

        #endregion
    }
}
