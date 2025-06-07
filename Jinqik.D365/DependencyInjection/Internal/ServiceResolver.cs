using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;

namespace Jinqik.D365.DependencyInjection.Internal
{
    public class ServiceResolver : IServiceResolver
    {
        private readonly Dictionary<Type, ServiceDescriptor> _serviceDescriptors;
        private readonly ConcurrentDictionary<Type, object> _contextCache;
        private readonly ConcurrentDictionary<Type, Func<object>> _factoryCache;
        private readonly object _factoryCompileLock = new object();
        private readonly HashSet<Type> _resolutionStack = new HashSet<Type>();
        private bool _disposed = false;
        private readonly ITracingService _tracingService;

        internal ServiceResolver(Dictionary<Type, ServiceDescriptor> serviceDescriptors, ITracingService tracingService)
        {
            _serviceDescriptors = serviceDescriptors ?? throw new ArgumentNullException(nameof(serviceDescriptors));
            _tracingService = tracingService;
            _contextCache = new ConcurrentDictionary<Type, object>();
            _factoryCache = new ConcurrentDictionary<Type, Func<object>>();
        }

        public object GetService(Type serviceType)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ServiceResolver));

            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            // Detect circular dependency
            if (_resolutionStack.Contains(serviceType))
                throw new InvalidOperationException(
                    $"Circular dependency detected for service type {serviceType.Name}");

            try
            {
                _resolutionStack.Add(serviceType);
                return GetServiceInternal(serviceType);
            }
            finally
            {
                _resolutionStack.Remove(serviceType);
            }
        }


        private object GetServiceInternal(Type serviceType)
        {
            return !_serviceDescriptors.TryGetValue(serviceType, out var descriptor)
                ? null
                : _contextCache.GetOrAdd(serviceType, _ => CreateInstance(descriptor));
        }

        private object CreateInstance(ServiceDescriptor descriptor)
        {
            if (descriptor.Instance != null)
            {
                return descriptor.Instance;
            }

            if (descriptor.Factory != null)
            {
                return descriptor.Factory(this);
            }

            if (descriptor.ImplementationType == null)
                throw new InvalidOperationException(
                    $"Invalid service descriptor for {descriptor.ServiceType.Name}. Must have Instance, Factory, or ImplementationType.");

            var factory = _factoryCache.GetOrAdd(descriptor.ImplementationType,
                _ => CompileFactory(descriptor.ImplementationType));
            return factory();
        }

        private Func<object> CompileFactory(Type implementationType)
        {
            lock (_factoryCompileLock)
            {
                if (_factoryCache.TryGetValue(implementationType, out var existingFactory))
                {
                    return existingFactory;
                }

                var constructors = implementationType.GetConstructors()
                    .Where(c => c.IsPublic)
                    .OrderByDescending(c => c.GetParameters().Length)
                    .ToArray();

                if (constructors.Length == 0)
                {
                    throw new InvalidOperationException($"No public constructor found for {implementationType.Name}");
                }

                foreach (var constructor in constructors)
                {
                    try
                    {
                        var factory = TryCompileConstructor(constructor);
                        if (factory == null) continue;
                        _factoryCache.TryAdd(implementationType, factory);
                        return factory;
                    }
                    catch
                    {
                        // ignored
                    }
                }

                throw new InvalidOperationException($"No suitable constructor found for {implementationType.Name}");
            }
        }

        private Func<object> TryCompileConstructor(System.Reflection.ConstructorInfo constructor)
        {
            var parameters = constructor.GetParameters();

            if (parameters.Length == 0)
            {
                // Không có dependency - tạo expression đơn giản
                var newExpression = Expression.New(constructor);
                var lambda = Expression.Lambda<Func<object>>(newExpression);
                return lambda.Compile();
            }
            else
            {
                // Kiểm tra tất cả dependencies có thể resolve được không
                foreach (var param in parameters)
                {
                    if (!_serviceDescriptors.ContainsKey(param.ParameterType))
                    {
                        // Nếu có parameter không thể resolve thì constructor này không dùng được
                        return null;
                    }
                }

                // Có dependency - tạo expression phức tạp
                var parameterExpressions = parameters.Select(param =>
                {
                    var getServiceCall = Expression.Call(
                        Expression.Constant(this),
                        typeof(ServiceResolver).GetMethod(nameof(GetService), new[] { typeof(Type) }) ??
                        throw new InvalidOperationException("GetService method not found"),
                        Expression.Constant(param.ParameterType));

                    return Expression.Convert(getServiceCall, param.ParameterType);
                }).Cast<Expression>().ToList();

                var constructorCall = Expression.New(constructor, parameterExpressions);
                var lambda = Expression.Lambda<Func<object>>(constructorCall);
                return lambda.Compile();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;


            foreach (var instance in _contextCache.Values)
            {
                if (!(instance is IDisposable disposable)) continue;
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    _tracingService.Trace("Exception caught during disposal");
                }
            }

            _contextCache.Clear();
            _factoryCache.Clear();
            _resolutionStack.Clear();
            _disposed = true;
        }
    }
}