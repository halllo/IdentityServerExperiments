using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MultiTenancy.Container
{
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
