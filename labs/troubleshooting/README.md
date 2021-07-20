# Troubleshooting Apps in Kubernetes

Observability is all about plugging your applications into collectors - Prometheus for metrics, Logstash for logs and Jeager for traces.

Those collectors are each part of their own subsystem, which need to plug into other components.

There is **a lot** of configuration between all these things, making sure the pieces can talk to each other and that they're sending information in the expected format to the expected location.

Plenty of things can go wrong in the setup, which will manifest as your apps missing logs or metrics, or parts of traces not being reported.

## Lab

This one is all lab :) There are four sets of containers to run:

- [metrics.yml](./metrics.yml) - Prometheus (we won't use Grafana this time round)
- [logging.yml](./logging.yml) - the ELK stack
- [tracing.yml](./tracing.yml) - Jaeger
- [apps.yml](./apps.yml) - application containers
- [load.yml](./load.yml) - a load-test tool ([Fortio]()), configured to send requests to the web app

Start all the containers:

```
docker-compose -f labs/troubleshooting/metrics.yml -f labs/troubleshooting/logging.yml -f labs/troubleshooting/tracing.yml -f labs/troubleshooting/apps.yml -f labs/troubleshooting/load.yml up -d
```

> Browse to the app at http://localhost:8070 and list the documents for the default user



Your goal is to get the observability stack working correctly and reporting everything:

- the metrics in Prometheus at http://localhost:9090 should show an `app_info` metric for every component instance:

![](../../img/troubleshooting-prometheus.png)

- the `Fulfilment.Authorization` service traces in Jaeger at http://localhost:16686 should show 10 spans:

![](../../img/troubleshooting-jaeger.png)

- build an area visualization in Kibana at http://localhost:5601 to show count of documents over time, split by `AppName` and you should see three applications writing lots of logs:

![](../../img/troubleshooting-kibana.png)

This is not meant to be a Docker troubleshooting exercise, but all the app and networking configuration is in Docker Compose. You'll need to fix things up by editing the application Compose file [app.yml](./app.yml) when you find the issues, then running this command:

```
docker-compose -f labs/troubleshooting/apps.yml up -d
```

Don't go straight to the solution! These are the sort of issues you will get all the time, so it's good to start working through the steps to diagnose problems.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___
## Cleanup

Cleanup by removing all containers:

```
docker rm -f $(docker ps -aq)
```