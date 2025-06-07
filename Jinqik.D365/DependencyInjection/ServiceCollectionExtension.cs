using System;
using System.Collections.Generic;
using Jinqik.D365.DependencyInjection.Internal;
using Microsoft.Xrm.Sdk;

// ReSharper disable MemberCanBePrivate.Global

namespace Jinqik.D365.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        #region Add by Type

        /// <summary>
        /// Adds a service of the type specified in TService with an implementation type specified in TImplementation
        /// </summary>
        public static IServiceCollection Add<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            return services.Add(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Adds a service of the type specified in serviceType with an implementation type specified in implementationType
        /// </summary>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType,
            Type implementationType)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            services.Add(new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType
            });

            return services;
        }

        /// <summary>
        /// Adds a service of the type specified in TService with the implementation type as the same type
        /// </summary>
        public static IServiceCollection Add<TService>(this IServiceCollection services)
            where TService : class
        {
            return services.Add(typeof(TService), typeof(TService));
        }

        /// <summary>
        /// Adds a service of the type specified in serviceType with the implementation type as the same type
        /// </summary>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType)
        {
            return services.Add(serviceType, serviceType);
        }

        #endregion

        #region Add by Factory

        /// <summary>
        /// Adds a service of the type specified in TService with a factory specified in implementationFactory
        /// </summary>
        public static IServiceCollection Add<TService>(this IServiceCollection services,
            Func<IServiceResolver, TService> implementationFactory)
            where TService : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            services.Add(new ServiceDescriptor
            {
                ServiceType = typeof(TService),
                Factory = implementationFactory
            });

            return services;
        }

        /// <summary>
        /// Adds a service of the type specified in serviceType with a factory specified in implementationFactory
        /// </summary>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType,
            Func<IServiceResolver, object> implementationFactory)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            services.Add(new ServiceDescriptor
            {
                ServiceType = serviceType,
                Factory = implementationFactory
            });

            return services;
        }

        #endregion

        #region Add by Instance

        /// <summary>
        /// Adds a service of the type specified in TService with an instance specified in implementationInstance
        /// </summary>
        public static IServiceCollection Add<TService>(this IServiceCollection services,
            TService implementationInstance)
            where TService : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationInstance == null) throw new ArgumentNullException(nameof(implementationInstance));

            services.Add(new ServiceDescriptor
            {
                ServiceType = typeof(TService),
                Instance = implementationInstance
            });

            return services;
        }

        /// <summary>
        /// Adds a service of the type specified in serviceType with an instance specified in implementationInstance
        /// </summary>
        public static IServiceCollection Add(this IServiceCollection services, Type serviceType,
            object implementationInstance)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (implementationInstance == null) throw new ArgumentNullException(nameof(implementationInstance));

            services.Add(new ServiceDescriptor
            {
                ServiceType = serviceType,
                Instance = implementationInstance
            });

            return services;
        }

        #endregion

        #region Try Add (only add if not already registered)

        /// <summary>
        /// Adds a service of the type specified in TService with an implementation type specified in TImplementation if not already registered
        /// </summary>
        public static IServiceCollection TryAdd<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
        {
            return services.TryAdd(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Adds a service of the type specified in serviceType with an implementation type specified in implementationType if not already registered
        /// </summary>
        public static IServiceCollection TryAdd(this IServiceCollection services, Type serviceType,
            Type implementationType)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            if (!services.IsRegistered(serviceType))
            {
                services.Add(serviceType, implementationType);
            }

            return services;
        }

        /// <summary>
        /// Adds a service of the type specified in TService with a factory if not already registered
        /// </summary>
        public static IServiceCollection TryAdd<TService>(this IServiceCollection services,
            Func<IServiceResolver, TService> implementationFactory)
            where TService : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            if (!services.IsRegistered<TService>())
            {
                services.Add(implementationFactory);
            }

            return services;
        }

        /// <summary>
        /// Adds a service instance if not already registered
        /// </summary>
        public static IServiceCollection TryAdd<TService>(this IServiceCollection services,
            TService implementationInstance)
            where TService : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationInstance == null) throw new ArgumentNullException(nameof(implementationInstance));

            if (!services.IsRegistered<TService>())
            {
                services.Add(implementationInstance);
            }

            return services;
        }

        #endregion

        #region Replace

        /// <summary>
        /// Replaces a service registration with a new implementation type
        /// </summary>
        public static IServiceCollection Replace<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
        {
            return services.Replace(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Replaces a service registration with a new implementation type
        /// </summary>
        public static IServiceCollection Replace(this IServiceCollection services, Type serviceType,
            Type implementationType)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            services.RemoveAll(serviceType);
            services.Add(serviceType, implementationType);

            return services;
        }

        /// <summary>
        /// Replaces a service registration with a new factory
        /// </summary>
        public static IServiceCollection Replace<TService>(this IServiceCollection services,
            Func<IServiceResolver, TService> implementationFactory)
            where TService : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (implementationFactory == null) throw new ArgumentNullException(nameof(implementationFactory));

            services.RemoveAll<TService>();
            services.Add(implementationFactory);

            return services;
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes all services of type TService
        /// </summary>
        public static IServiceCollection RemoveAll<TService>(this IServiceCollection services)
        {
            return services.RemoveAll(typeof(TService));
        }

        /// <summary>
        /// Removes all services of the specified type
        /// </summary>
        public static IServiceCollection RemoveAll(this IServiceCollection services, Type serviceType)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            for (int i = services.Count - 1; i >= 0; i--)
            {
                if (services[i].ServiceType == serviceType)
                {
                    services.RemoveAt(i);
                }
            }

            return services;
        }

        #endregion

        #region Query

        /// <summary>
        /// Checks if a service of type TService is registered
        /// </summary>
        public static bool IsRegistered<TService>(this IServiceCollection services)
        {
            return services.IsRegistered(typeof(TService));
        }

        /// <summary>
        /// Checks if a service of the specified type is registered
        /// </summary>
        public static bool IsRegistered(this IServiceCollection services, Type serviceType)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            foreach (var descriptor in services)
            {
                if (descriptor.ServiceType == serviceType)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the service descriptor for the specified service type
        /// </summary>
        public static ServiceDescriptor GetDescriptor<TService>(this IServiceCollection services)
        {
            return services.GetDescriptor(typeof(TService));
        }

        /// <summary>
        /// Gets the service descriptor for the specified service type
        /// </summary>
        public static ServiceDescriptor GetDescriptor(this IServiceCollection services, Type serviceType)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            foreach (var descriptor in services)
            {
                if (descriptor.ServiceType == serviceType)
                {
                    return descriptor;
                }
            }

            return null;
        }

        #endregion

        public static IServiceResolver BuildResolver(this IServiceCollection services, ITracingService tracingService)
        {
            if (tracingService == null) throw new ArgumentNullException(nameof(tracingService));
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.Add(tracingService);

            // Convert IServiceCollection to Dictionary<Type, ServiceDescriptor>
            var serviceDescriptors = new Dictionary<Type, ServiceDescriptor>();

            foreach (var descriptor in services)
            {
                if (descriptor.ServiceType == null)
                    throw new InvalidOperationException("ServiceDescriptor must have a ServiceType");

                // Last registration wins
                serviceDescriptors[descriptor.ServiceType] = descriptor;
            }

            return new ServiceResolver(serviceDescriptors, tracingService);
        }
    }
}