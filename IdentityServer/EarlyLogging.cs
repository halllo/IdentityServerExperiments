using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer
{
    public static class EarlyLogging
    {
        public static void AddEarlyLogging(this IServiceCollection services, out ILogger earlyLogger) => earlyLogger = AddEarlyLogging(services);
        public static ILogger AddEarlyLogging(this IServiceCollection services)
        {
            var earlyLogger = new EarlyLogger();
            services.AddSingleton<IStartupFilter>(sp => new EarlyLoggingStartupFilter(earlyLogger, sp.GetRequiredService<ILogger<EarlyLogger>>()));
            return earlyLogger;
        }

        private class EarlyLogger : ILogger, IDisposable
        {
            public readonly Queue<(LogLevel level, EventId eventId, Type tstate, object state, Exception exception, object formatter)> logs;
            private bool isDisposed = false;

            public EarlyLogger()
            {
                this.logs = new Queue<(LogLevel, EventId, Type, object, Exception, object)>();
            }

            public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
            public bool IsEnabled(LogLevel logLevel) => throw new NotImplementedException();
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(EarlyLogger));
                this.logs.Enqueue((logLevel, eventId, typeof(TState), state, exception, formatter));
            }

            public void Dispose()
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(EarlyLogger));
                isDisposed = true;
            }
        }

        private class EarlyLoggingStartupFilter : IStartupFilter
        {
            private readonly EarlyLogger earlyLogger;
            private readonly ILogger<EarlyLogger> logger;

            public EarlyLoggingStartupFilter(EarlyLogger earlyLogger, ILogger<EarlyLogger> logger)
            {
                this.earlyLogger = earlyLogger;
                this.logger = logger;
            }

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                var logTState = typeof(ILogger).GetMethods().Where(m => m.Name == nameof(ILogger.Log) && m.ContainsGenericParameters).Single();

                while (this.earlyLogger.logs.Count > 0)
                {
                    var log = this.earlyLogger.logs.Dequeue();
                    logTState.MakeGenericMethod(log.tstate).Invoke(this.logger, new object[]
                    {
                        log.level, log.eventId, log.state, log.exception, log.formatter
                    });
                }
                this.earlyLogger.Dispose();

                return next;
            }
        }
    }
}
