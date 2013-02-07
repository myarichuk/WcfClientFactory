using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core;
using Castle.MicroKernel.Facilities;
using System.ServiceModel;

namespace WcfClientFactory.CastleWindsor
{
    public class WcfClientFactoryFacility : AbstractFacility
    {
        private readonly Dictionary<Type, Type> m_ClientBaseTypesByInterface;

        public WcfClientFactoryFacility()
        {
            m_ClientBaseTypesByInterface = new Dictionary<Type, Type>();
        }

        public void RegisterInterface<TInterface>()
            where TInterface : class
        {
            m_ClientBaseTypesByInterface.Add(typeof(TInterface),typeof(ClientBase<TInterface>));
        }

        public void RegisterInterface<TInterface,TBaseClass>()
            where TInterface : class
            where TBaseClass : ClientBase<TInterface>
        {
            m_ClientBaseTypesByInterface.Add(typeof(TInterface), typeof(TBaseClass));
        }

        //TODO: replace tuple with strongly typed DTO of (TInterface,TBaseClass) types
        public void RegisterMultipleInterfaces(params Tuple<Type, Type>[] args)
        {
        }

        protected override void Init()
        {                        
            //TODO: 
        }
    }
}
