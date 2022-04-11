using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.Http.Dependencies;

using Ninject;
using Ninject.Syntax;

namespace OMS.API
{
    public class NinjectConfig
    {
        private static readonly IKernel Kernel;
        static NinjectConfig()
        {
            Kernel = new StandardKernel();
            AddBindings();
        }

        public static void RegisterNinject(HttpConfiguration config)
        {
            config.DependencyResolver = new NinjectDependencyResolver(Kernel);
        }

        private static void AddBindings()
        {
            //Warehouse
            Kernel.Bind<Interface.Warehouse.IQueryService>().To<Implments.Warehouse.QueryService>();
            Kernel.Bind<Interface.Warehouse.IPostService>().To<Implments.Warehouse.PostService>();

            //ClickCollect
            Kernel.Bind<Interface.ClickCollect.IQueryService>().To<Implments.ClickCollect.QueryService>();
            Kernel.Bind<Interface.ClickCollect.IPostService>().To<Implments.ClickCollect.PostService>();

            //Platform
            Kernel.Bind<Interface.Platform.IQueryService>().To<Implments.Platform.QueryService>();
            Kernel.Bind<Interface.Platform.IPostService>().To<Implments.Platform.PostService>();
        }
    }

    public class NinjectDependencyResolver : NinjectDependencyScope, IDependencyResolver
    {
        private IKernel kernel;

        public NinjectDependencyResolver(IKernel kernel) : base(kernel)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException("kernel");
            }
            this.kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectDependencyScope(kernel);
        }
    }

    public class NinjectDependencyScope : IDependencyScope
    {
        private IResolutionRoot resolver;

        internal NinjectDependencyScope(IResolutionRoot resolver)
        {
            Contract.Assert(resolver != null);

            this.resolver = resolver;
        }

        public void Dispose()
        {
            resolver = null;
        }

        public object GetService(Type serviceType)
        {
            return resolver.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return resolver.GetAll(serviceType);
        }
    }
}