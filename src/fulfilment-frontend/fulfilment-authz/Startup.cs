using Fulfilment.Core.Application;
using Fulfilment.Core.Configuration;
using Fulfilment.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddLogging(Configuration, _options);
            services.AddTracing(_options.Trace);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseMetrics(_options.Metrics);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }        
    }
}
