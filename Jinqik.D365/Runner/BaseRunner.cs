using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Jinqik.D365.DependencyInjection;
using Jinqik.D365.DependencyInjection.Internal;
using Jinqik.D365.Exceptions;
using Jinqik.D365.Services;
using Microsoft.Xrm.Sdk;

namespace Jinqik.D365.Runner
{
    internal class BaseRunner
    {
        private readonly IServiceProvider _serviceProvider;

        public BaseRunner(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Run(Action<IServiceCollection, IServiceProvider> setupAction,
            Action<IServiceResolver> executeAction)
        {
            var startTime = DateTime.UtcNow;
            var tracingService = _serviceProvider.GetService<ITracingService>();
            var pluginExecutionContext = _serviceProvider.GetService<IPluginExecutionContext>();

            tracingService.Trace(string.Format(
                "[{0:o}] Correlation Id:{1}, Initiating User:{2}, User:{3}",
                startTime,
                pluginExecutionContext.CorrelationId,
                pluginExecutionContext.InitiatingUserId,
                pluginExecutionContext.UserId));
            var serviceCollection = new ServiceCollection();
            serviceCollection.Add<IPluginExecutionContext>(pluginExecutionContext);
            var orgServiceFactory = _serviceProvider.GetService<IOrganizationServiceFactory>();
            serviceCollection.Add(orgServiceFactory);
            serviceCollection.Add<IMessageService, NullMessageService>();
            setupAction.Invoke(serviceCollection, _serviceProvider);
            using (var serviceResolver = serviceCollection.BuildResolver(tracingService))
            {
                var time = DateTime.UtcNow - startTime;
                tracingService.Trace($"[{DateTime.UtcNow:o}] Setup Completed:({time.Milliseconds})");
                try
                {
                    executeAction?.Invoke(serviceResolver);
                }
                catch (BusinessException businessException)
                {
                    var messageService = serviceResolver.GetService(typeof(IMessageService)) as IMessageService;
                    var message = messageService.GetMessage(businessException.MessageCode);
                    if (businessException.Parameters != null && businessException.Parameters.Any())
                    {
                        message = string.Format(message, businessException.Parameters);
                    }

                    throw new InvalidPluginExecutionException(message);
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    tracingService.Trace(string.Format(CultureInfo.InvariantCulture, "Exception {0}", ex.ToString()));
                    tracingService.Trace(string.Format(CultureInfo.InvariantCulture, "Stack Trace {0}", ex.StackTrace));
                    throw new InvalidPluginExecutionException(ex.Message, ex);
                }
                catch (Exception ex)
                {
                    tracingService.Trace(
                        string.Format(CultureInfo.InvariantCulture, "Exception {0}", ex.ToString()));
                    tracingService.Trace(string.Format(CultureInfo.InvariantCulture, "Stack Trace {0}",
                        ex.StackTrace));
                    throw new InvalidPluginExecutionException("An unexpected error has occurred", ex);
                }
                finally
                {
                    tracingService.Trace($"[{DateTime.UtcNow:o}] Completed");
                }
            }
        }
    }
}