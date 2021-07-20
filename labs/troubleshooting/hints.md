# Lab Hints

It's best to approach this one part at a time, and get all the instrumentation for one level working before you move onto the next.

There aren't any deliberate errors in the applications, so they'll keep running - but they won't be instrumented correctly to start with.

The load-generator containers will keep making HTTP requests so you'll have lots of data to work with, but if it's too much and you'd rather make manual calls then you can stop the Fortio containers:

```
docker-compose -f labs/troubleshooting/load.yml stop
```

## Troubleshooting Metrics

The Prometheus UI is a good place to start. The target list at http://localhost:9090/classic/targets will tell you where Prometheus is collecting metrics from and whether the targets are up.

Potential issues here are in the application configuration in [app.yml](./app.yml), or in the Prometheus configuration in [config/prometheus.yml](./config/prometheus.yml).

## Troubleshooting Traces

When you search in the Jaeger UI you're searching for spans, but when you open a span to visualize it you're seeing the whole trace.

If spans are missing that's likely to be an application config issue - not reporting metrics, or using an incorrect tracing format (W3C is the common standard now, but B3 is still used and the two are not compatible).

## Troubleshooting Logs

You can import the saved visualization and index pattern from `solution/kibana.ndjson` to see the split of logs coming in.

If logs are missing for components it could be a logging configuration issue in [app.yml](./app.yml), or an issue with one of the Logstash pipelines in the `pipelines` folder, or an issue with the Logstash configuration in [config/pipelines.yml](./config/pipelines.yml).

> Need more? Here's the [solution](solution.md).