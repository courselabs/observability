using Fulfilment.Web.Configuration;
using Fulfilment.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

            services.AddTransient<DocumentsService>();

            services.AddOpenTelemetryTracing(builder => 
            {
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Fulfilment.Web"))
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
