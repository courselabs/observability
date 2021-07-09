using Fulfilment.Processor.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;
using Serilog;
using Serilog.Templates;
using System;
using System.Threading;

namespace Fulfilment.Processor
{
    class Program
    {
        private static CancellationTokenSource _Cancellation = new CancellationTokenSource();
        private static readonly Random _Random = new Random();

        private static readonly Counter _ProcessedCounter = Metrics.CreateCounter("fulfilment_requests_total", "Fulfilment requests received", "status");
        private static readonly Gauge _InProgressGauge = Metrics.CreateGauge("fulfilment_in_flight_total", "Fulfilment requests in progress");

        private static Microsoft.Extensions.Logging.ILogger _Log;
        private static bool _StructuredLogging = false;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                _Cancellation.Cancel();
            };

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json")
                         .AddEnvironmentVariables()                 
                         .AddJsonFile("config/override.json", optional: true, reloadOnChange: true);

            var config = configBuilder.Build();
            _StructuredLogging = config.GetValue<bool>("Observability:Logging:Structured");
            var writeConsole = config.GetValue<bool>("Observability:Logging:Console");
            var writeFile = config.GetValue<bool>("Observability:Logging:File");

            // configure Serilog
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(config);
            if (_StructuredLogging)
            {
                var formatter = new ExpressionTemplate("{ {Timestamp: @t, Entry: @m, Level: @l, Exception: @x, ..@p} }\n");
                if (writeFile)
                {
                    loggerConfig.WriteTo.File(
                        formatter,
                        "logs/fulfilment-processor.json",
                        shared: true);
                }
                if (writeConsole)
                {
                    loggerConfig.WriteTo.Console(formatter);
                }
            }
            else
            {
                if (writeFile)
                {
                    loggerConfig.WriteTo.File("logs/fulfilment-processor.log", shared: true);
                }
                if (writeConsole)
                {
                    loggerConfig.WriteTo.Console();
                }
            }

            var logger = loggerConfig.Enrich.WithAppVersion().CreateLogger();

            // configure logging
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));

            var serviceProvider = services.BuildServiceProvider();
            _Log = serviceProvider.GetRequiredService<ILogger<Program>>();

            if (config.GetValue<bool>("Observability:Metrics:Enabled"))
            {
                var server = new MetricServer(port: 5000);
                server.Start();
            }

            while (!_Cancellation.Token.IsCancellationRequested)
            {
                var processed = _Random.Next(0, 200);
                var failed = _Random.Next(0, 15);

                RecordProcessed(processed, failed);
                RecordFailed(processed, failed);

                var inFlight = _Random.Next(0, 100);
                _InProgressGauge.Set(inFlight);

                _Cancellation.Token.WaitHandle.WaitOne(_Random.Next(1, 10) * 1000);
            }
        }

        private static void RecordProcessed(int processed, int failed)
        {
            _ProcessedCounter.WithLabels(nameof(processed)).Inc(processed);
            for (int i = 0; i < processed - failed; i++)
            {
                var requestId = _Random.Next(20000000, 40000000);
                var duration = _Random.Next(2000, 12000);
                if (_StructuredLogging)
                {
                    _Log.LogTrace("{EventType}: Request ID: {RequestId}", EventType.Requested, requestId);
                    _Log.LogDebug("{EventType}: Request ID: {RequestId}", EventType.InFlight, requestId);
                    _Log.LogDebug("{EventType}: Request ID: {RequestId}. Took: {Duration}ms.", EventType.Processed, requestId, duration);
                }
                else
                {
                    _Log.LogTrace($"Fulfilment requested. Request ID: {requestId}");
                    _Log.LogDebug($"Fulfilment in-flight. Request ID: {requestId}");
                    _Log.LogDebug($"Fulfilment request processed. Request ID: {requestId}. Took: {duration}ms.");
                }
            }
        }

        private static void RecordFailed(int processed, int failed)
        {
            _ProcessedCounter.WithLabels(nameof(failed)).Inc(failed);

            for (int i = 0; i < failed; i++)
            {
                var requestId = _Random.Next(30000000, 35000000);

                var errorMessage = ErrorMessage.Unavailable;
                if (i == 15 && processed > 150)
                {
                    errorMessage = ErrorMessage.NoPaper;
                }
                else if (i > 10 && processed > 100)
                {
                    errorMessage = ErrorMessage.Code302;
                }

                if (_StructuredLogging)
                {
                    _Log.LogTrace("{EventType}: Request ID: {RequestId}", EventType.Requested, requestId);
                    _Log.LogError("{EventType}: Request ID: {RequestId}. Error: {ErrorMessage}", EventType.Failed, requestId, errorMessage);
                }
                else
                {
                    _Log.LogTrace($"Fulfilment requested. Request ID: {requestId}");
                    _Log.LogError($"Fulfilment error! Request ID: {requestId}. Error: {errorMessage}");
                }
            }
        }

        private struct EventType
        {
            public const string Processed = "Fulfilment.Processed";
            public const string Failed = "Fulfilment.Failed";
            public const string Requested = "Fulfilment.Requested";
            public const string InFlight = "Fulfilment.InFlight";
        }

        private struct ErrorMessage
        {
            public const string Unavailable = "Document service unavailable";
            public const string Code302 = "Document service error code 302";
            public const string NoPaper = "Out of paper. Please load plain A4 into tray 1";
        }
    }
}