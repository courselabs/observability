using Fulfilment.Web.Configuration;
using Fulfilment.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Templates;
using System.Diagnostics;

namespace Fulfilment.Web
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
            services.AddRazorPages();
            services.AddHttpClient();

            // configure Serilog
            var logger = CreateLogger(Configuration);
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));

            services.AddSingleton(_options);
            services.AddTransient<DocumentsService>();

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Activity.ForceDefaultIdFormat = true;

            services.AddOpenTelemetryTracing(builder => 
            {
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Fulfilment.Web"))
                       .AddSource("Fulfilment.Web")
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

            services.AddSingleton(new ActivitySource("Fulfilment.Web", "1.0.0"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }

        private static Serilog.Core.Logger CreateLogger(IConfiguration config)
        {
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
            var formatter = new ExpressionTemplate("{ {Timestamp: @t, Entry: @m, Level: @l, Exception: @x, ..@p} }\n");

            loggerConfig.WriteTo.File(
                    formatter,
                    "logs/fulfilment-web.json",
                    shared: true);

            return loggerConfig.CreateLogger();
        }
    }
}
