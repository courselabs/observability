using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;

namespace Fulfilment.Core.Services
{
    public static class Tracing
    {
        public static IServiceCollection AddTracing(this IServiceCollection services, Configuration.TraceOptions options)
        {
            ConfigureActivity(options);

            var assembly = Assembly.GetEntryAssembly().GetName();
            services.AddOpenTelemetryTracing(builder =>
            {
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(assembly.Name))
                       .AddSource(assembly.Name)
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation();

                if (options.Console)
                {
                    builder.AddConsoleExporter();
                }
                if (options.Jaeger)
                {
                    builder.AddJaegerExporter(opts =>
                    {
                        opts.AgentHost = options.Agent.Host;
                        opts.AgentPort = options.Agent.Port;
                    });
                }
            });
            
            return services.AddSingleton(new ActivitySource($"{assembly.Name}", $"{assembly.Version}"));            
        }

        private static void ConfigureActivity(Configuration.TraceOptions options)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            if (options.Baggage.Tag)
            {
                var listener = new ActivityListener
                {
                    ShouldListenTo = _ => true,
                    ActivityStopped = activity =>
                    {
                        foreach (var (key, value) in activity.Baggage)
                        {
                            activity.AddTag(key, value);
                        }
                    }
                };
                ActivitySource.AddActivityListener(listener);
            }
        }
    }
}
