using Fulfilment.Authorization.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fulfilment.Authorization
{
    public class Startup
    {
        private readonly ObservabilityOptions _options = new ObservabilityOptions();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            configuration.GetSection("Observability").Bind(_options);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllers();

            services.AddOpenTelemetryTracing(builder =>
            {
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Fulfilment.Authz"))
                       .AddAspNetCoreInstrumentation()
                       .AddHttpClientInstrumentation();

                if (_options.Trace.Console)
                {
                    builder.AddConsoleExporter();
                }
                if (_options.Trace.Jaeger)
                {
                    builder.AddJaegerExporter(opts =>
                    {
                        opts.AgentHost = _options.Trace.Agent.Host;
                        opts.AgentPort = _options.Trace.Agent.Port;
                    });
                }
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
