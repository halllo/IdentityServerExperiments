using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using Autofac.Extensions.DependencyInjection;
using MultiTenancy.Resolution;

namespace MultiTenancy.Container
{

	internal class MultiTenantContainer<T> : IContainer, IMultiTenantContainer where T : Tenant
	{
		//This is the base application container
		private readonly IContainer _applicationContainer;
		//This action configures a container builder
		private readonly Action<T, ContainerBuilder, IComponentContext> _tenantContainerConfiguration;

		//This dictionary keeps track of all of the tenant scopes that we have created
		private readonly Dictionary<string, ILifetimeScope> _tenantLifetimeScopes = new Dictionary<string, ILifetimeScope>();

		private readonly object _lock = new object();
		private const string _multiTenantTag = "multitenantcontainer";

		public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning;
		public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding;
		public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning;

		public MultiTenantContainer(IContainer applicationContainer, Action<T, ContainerBuilder, IComponentContext> containerConfiguration)
		{
			_tenantContainerConfiguration = containerConfiguration;
			_applicationContainer = applicationContainer;
		}

		public IDisposer Disposer => GetCurrentTenantScope().Disposer;

		public object Tag => GetCurrentTenantScope().Tag;

		public IComponentRegistry ComponentRegistry => GetCurrentTenantScope().ComponentRegistry;

		public ILifetimeScope GetCurrentTenantScope()
		{
			return GetTenantScope(GetCurrentTenant()?.Name);
		}

		public ILifetimeScope GetTenantScope(string tenantName)
		{
			//If no tenant (e.g. early on in the pipeline, we just use the application container)
			if (tenantName == null)
				return _applicationContainer;

			//If we have created a lifetime for a tenant, return
			if (_tenantLifetimeScopes.ContainsKey(tenantName))
				return _tenantLifetimeScopes[tenantName];

			lock (_lock)
			{
				if (_tenantLifetimeScopes.ContainsKey(tenantName))
				{
					return _tenantLifetimeScopes[tenantName];
				}
				else
				{
					//This is a new tenant, configure a new lifetimescope for it using our tenant sensitive configuration method
					_tenantLifetimeScopes.Add(tenantName, _applicationContainer.BeginLifetimeScope(_multiTenantTag, a => _tenantContainerConfiguration(GetTenant(tenantName), a, _applicationContainer)));
					return _tenantLifetimeScopes[tenantName];
				}
			}
		}

		private T GetCurrentTenant()
		{
			return _applicationContainer.Resolve<ITenantAccessor<T>>().Tenant;
		}

		private T GetTenant(string identifier)
		{
			return _applicationContainer.Resolve<ITenantStore<T>>().GetTenantAsync(identifier).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public void Dispose()
		{
			lock (_lock)
			{
				foreach (var scope in _tenantLifetimeScopes)
					scope.Value.Dispose();
				_applicationContainer.Dispose();
			}
		}

		public ILifetimeScope BeginLifetimeScope()
		{
			throw new NotImplementedException();
		}

		public ILifetimeScope BeginLifetimeScope(object tag)
		{
			return GetCurrentTenantScope().BeginLifetimeScope(tag);
		}

		public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
		{
			return GetCurrentTenantScope().BeginLifetimeScope(configurationAction);
		}

		public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
		{
			return GetCurrentTenantScope().BeginLifetimeScope(tag, configurationAction);
		}

		public object ResolveComponent(ResolveRequest request)
		{
			return GetCurrentTenantScope().ResolveComponent(request);
		}

		public ValueTask DisposeAsync()
		{
			return GetCurrentTenantScope().DisposeAsync();
		}

		IServiceProvider IMultiTenantContainer.GetTenantContainer(string tenantName) => new AutofacServiceProvider(GetTenantScope(tenantName).BeginLifetimeScope());
	}
}
