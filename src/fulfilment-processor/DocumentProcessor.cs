using Fulfilment.Processor.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using System;
using System.Threading;

namespace Fulfilment.Processor
{
    public class DocumentProcessor
    {
        private static readonly Random _Random = new Random();

        private static readonly Counter _ProcessedCounter = Metrics.CreateCounter("fulfilment_requests_total", "Fulfilment requests received", "status");
        private static readonly Gauge _InProgressGauge = Metrics.CreateGauge("fulfilment_in_flight_total", "Fulfilment requests in progress");
        private static readonly Histogram  _ProcessDurationHistogram = Metrics.CreateHistogram("fulfilment_processing_seconds", "Fulfilment processing duration",
            new HistogramConfiguration
            {
                // 2s buckets, from 0s to 14s
                Buckets = Histogram.LinearBuckets(start: 0, width: 2, count: 7)
            }); 
        

        private readonly ObservabilityOptions _options;
        private readonly ILogger _logger;

        public DocumentProcessor(IOptions<ObservabilityOptions> options, ILogger<DocumentProcessor> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public void GenerateRandom(CancellationTokenSource cancellation)
        {
            while (!cancellation.Token.IsCancellationRequested)
            {
                var inFlight = GenerateMetric(0, 200);
                _InProgressGauge.Set(inFlight);

                var failed = _Random.Next(0, (int) Math.Round(inFlight * _options.Metrics.FailureFactor));
                RecordProcessed(inFlight, failed);
                RecordFailed(inFlight, failed);               

                cancellation.Token.WaitHandle.WaitOne(_Random.Next(1, 10) * 1000);
            }
        }

        private int GenerateMetric(int from, int to)
        {
            return (int) Math.Round(_Random.Next(from, to) * _options.Metrics.Factor);
        }

        private void RecordProcessed(int processed, int failed)
        {
            _ProcessedCounter.WithLabels(nameof(processed)).Inc(processed);
            for (int i = 0; i < processed - failed; i++)
            {
                var requestId = _Random.Next(20000000, 40000000);
                var duration = _Random.Next(2000, 12000);
                _ProcessDurationHistogram.Observe(duration/1000);
                if (_options.Logging.Structured)
                {
                    _logger.LogTrace("{EventType}: Request ID: {RequestId}", EventType.Requested, requestId);
                    _logger.LogDebug("{EventType}: Request ID: {RequestId}", EventType.InFlight, requestId);
                    _logger.LogDebug("{EventType}: Request ID: {RequestId}. Took: {Duration}ms.", EventType.Processed, requestId, duration);
                }
                else
                {
                    _logger.LogTrace($"Fulfilment requested. Request ID: {requestId}");
                    _logger.LogDebug($"Fulfilment in-flight. Request ID: {requestId}");
                    _logger.LogDebug($"Fulfilment request processed. Request ID: {requestId}. Took: {duration}ms.");
                }
            }
        }

        private void RecordFailed(int processed, int failed)
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

                if (_options.Logging.Structured)
                {
                    _logger.LogTrace("{EventType}: Request ID: {RequestId}", EventType.Requested, requestId);
                    _logger.LogError("{EventType}: Request ID: {RequestId}. Error: {ErrorMessage}", EventType.Failed, requestId, errorMessage);
                }
                else
                {
                    _logger.LogTrace("Fulfilment requested. Request ID: " + requestId);
                    _logger.LogError("Fulfilment error! Request ID: " + requestId +". Error: " + errorMessage);
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
