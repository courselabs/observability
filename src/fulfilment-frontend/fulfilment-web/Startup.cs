using Fulfilment.Core.Configuration;
using Fulfilment.Core.Services;
using Fulfilment.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddRazorPages()
                    .AddRazorPagesOptions(options =>
                    {
                        options.Conventions
                            .ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                    });

            services.AddHttpClient("client");

            services.AddSingleton(_options);
            services.AddTransient<AuthorizationService>();
            services.AddTransient<SubmitDocumentService>();
            services.AddTransient<ListDocumentsService>();

            services.AddLogging(Configuration, _options.Trace);
            services.AddTracing(_options.Trace);
            services.AddBaggageHeaderPropagation();
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
            app.UseHeaderPropagation();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
