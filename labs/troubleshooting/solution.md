# Lab Solution

There's a lot to fix here, but these are all common issues so it's good to walk through them and see how missing instrumentation is usually a configuration issue.

The fixes are all in the `solution` folder. 

- [prometheus.yml](./solution/prometheus.yml) - fixes the Prometheus scrape configuration
- [metrics.yml](./solution/metrics.yml) - loads the fixed config file into Prometheus
- [fulfilment-authz.conf](./solution/pipelines/fulfilment-authz.conf) - uses the correct log output
- [pipelines.yml](./solution/pipelines.yml) - loads all the pipelines into Logstash
- [logging.yml](./solution/logging.yml) - loads the fixed config file into Logstash
- [app.yml](./solution/apps.yml) - fixes all the app misconfigurations

Deploy the changes and the app will produce all the expected instrumentation:

```
docker-compose -f labs/troubleshooting/solution/metrics.yml -f labs/troubleshooting/solution/logging.yml  -f labs/troubleshooting/solution/apps.yml up -d
```

> The load-generator containers will keep making HTTP requests so you'll have lots of data to work with, but if that's making your machine work too hard, you can stop the Fortio containers and make manual website calls instead:

```
docker-compose -f labs/troubleshooting/load.yml stop
```

## Troubleshooting Metrics

Fixes:

1. Authorization service misconfigured - not producing metrics
    - the configuration setting `Observability__Metrics__On` is incorrect - it should be `Observability__Metrics__Enabled` like the other components

2. Web application not found in Prometheus scrape
    - the web container is not connected to the `metrics` network - you may have observability sub-systems in their own networks, and you need to make sure there's connectivity from your application

3. Fulfilment processor metrics not found
    - the Prometheus scrape configuration is incorrect, it's trying to access metrics at the path `/all-the-metrics` - it should be using just `/metrics`

## Troubleshooting Traces

Fixes:
    
1. Documents service spans not being reported
    - the API app is misconfigured - it's using a hostname of `metrics` to send data to Jaeger, the correct hostname is `jaeger`

2. Spans are not being linked to the parent trace
    - the web app is configured incorrectly - it's using the B3 header format with the setting `Observability__Trace__HeaderFormat=B3`; the other components are using W3C and the formats are not compatible so the trace can't be built. This setting can be removed, or the value replaced with `W3C`.

## Troubleshooting Logs


1. Authorization logs are missing
    - Logstash is misconfigured - only two pipelines are loaded, so the authorization service log files are not collected

2. Authorization logs are **still**  missing
    - the Logstash pipeline is also misconfigured, it's using a console output where it should be using Elasticsearch

3. Fulfilment processor instance 2's logs are missing
    - the application is misconfigured. The pipeline expects log file names in the pattern `fulfilment-processor*.json` - the config setting `Observability__Logging__LogFilePath=logs/fulfilment-2.json` uses a log file name which doesn't match the pattern.