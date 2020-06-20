using System;
using System.Collections;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy.Container
{
    public class MultiTenantServiceProviderFactory<T> : IServiceProviderFactory<ContainerBuilder> where T : Tenant
    {

        private readonly Action<T, ContainerBuilder, IComponentContext> _tenantSerivcesConfiguration;

        public MultiTenantServiceProviderFactory(Action<T, ContainerBuilder, IComponentContext> tenantSerivcesConfiguration)
        {
            _tenantSerivcesConfiguration = tenantSerivcesConfiguration;
        }

        /// <summary>
        /// Create a builder populated with global services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            builder.Populate(services);

            return builder;
        }

        /// <summary>
        /// Create our serivce provider
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <returns></returns>
        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            MultiTenantContainer<T> container = null;

            Func<MultiTenantContainer<T>> internalContainerAccessor = () =>
            {
                return container;
            };
            containerBuilder.RegisterInstance(internalContainerAccessor).SingleInstance();

            Func<IMultiTenantContainer> publicContainerAccessor = () =>
            {
                return container;
            };
            containerBuilder.RegisterInstance(publicContainerAccessor).SingleInstance();

            container = new MultiTenantContainer<T>(containerBuilder.Build(), _tenantSerivcesConfiguration);

            return new AutofacServiceProvider(internalContainerAccessor());
        }
    }







    /// <summary>
    /// Can be used with <see cref="new WebHostBuilder().ConfigureServices(...)"/> so even ServiceFabric can use <see cref="IServiceProviderFactory<TContainerBuilder>"/> generic factories.
    /// </summary>
    /// <typeparam name="TContainerBuilder"></typeparam>
    public class ServiceProviderFactoryGenericAdapter<TContainerBuilder> : IServiceProviderFactory<IServiceCollection>
    {
        private readonly IServiceProviderFactory<TContainerBuilder> factory;

        public ServiceProviderFactoryGenericAdapter(IServiceProviderFactory<TContainerBuilder> factory)
        {
            this.factory = factory;
        }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            TContainerBuilder containerBuilder = factory.CreateBuilder(services);
            return new ContainerBuilderAdapter(containerBuilder);
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection services)
        {
            var containerBuilder = ((ContainerBuilderAdapter)services).Adaptee;
            var serviceProvider = factory.CreateServiceProvider(containerBuilder);
            return serviceProvider;
        }

        private class ContainerBuilderAdapter : IServiceCollection
        {
            public TContainerBuilder Adaptee { get; }
            public ContainerBuilderAdapter(TContainerBuilder adaptee)
            {
                Adaptee = adaptee;
            }

            public ServiceDescriptor this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public int Count => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public void Add(ServiceDescriptor item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(ServiceDescriptor item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<ServiceDescriptor> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public int IndexOf(ServiceDescriptor item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, ServiceDescriptor item)
            {
                throw new NotImplementedException();
            }

            public bool Remove(ServiceDescriptor item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }
    }
}
