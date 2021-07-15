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
using Serilog;
using Serilog.Templates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            // configure Serilog
            var logger = CreateLogger(Configuration);
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

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

        private static Serilog.Core.Logger CreateLogger(IConfiguration config)
        {
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
            var formatter = new ExpressionTemplate("{ {Timestamp: @t, Entry: @m, Level: @l, Exception: @x, ..@p} }\n");

            loggerConfig.WriteTo.File(
                    formatter,
                    "logs/fulfilment-authz.json",
                    shared: true);

            return loggerConfig.CreateLogger();
        }
    }
}
