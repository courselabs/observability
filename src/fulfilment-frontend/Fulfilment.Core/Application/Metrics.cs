using Microsoft.AspNetCore.Builder;
using Prometheus;
using System.Reflection;

namespace Fulfilment.Core.Application
{
    public static class Metrics
    {
        private static bool _WrittenAppInfo;

        public static IApplicationBuilder UseMetrics(this IApplicationBuilder app, Configuration.MetricsOptions metricsOptions)
        {
            if (!metricsOptions.IncludeRuntime)
            {
                Prometheus.Metrics.SuppressDefaultMetrics();
            }
            if (metricsOptions.Enabled)
            {
                app.UseMetricServer();
                app.UseHttpMetrics();
                EnsureAppInfo();
            }
            return app;
        }

        private static void EnsureAppInfo()
        {
            if (!_WrittenAppInfo)
            {
                var assembly = Assembly.GetEntryAssembly().GetName();
                var infoGauge = Prometheus.Metrics.CreateGauge("app_info", "Application info", "dotnet_version", "assembly_name", "app_version");
                infoGauge.Labels("3.1.16", assembly.Name, $"{assembly.Version}").Set(1);
                _WrittenAppInfo = true;
            }
        }
    }
}
