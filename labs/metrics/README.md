
# Exporting Metrics




## Application Metrics

docker run -d -p 9110:9110 --name processor courselabs/obsfun-fulfilment-processor

http://localhost:9110/metrics

Refresh; what's the difference between counter and gauge types?

includes processed and failed but not succeeded - why?

docker rm -f processor

docker run -d -p 9110:9110 --name processor -e Observability__Metrics__IncludeRuntime=true courselabs/obsfun-fulfilment-processor

http://localhost:9110/metrics

> Standard metrics, CPU time spent & memory usage

## Server metrics

docker run -d -p 9100:9100 --name node courselabs/obsfun-node-exporter

http://localhost:9100/metrics

System info - disk etc. What language? Any common metrics with ^^?

## Runtime metrics

docker run -d -p 8080:80 courselabs/obsfun-fulfilment-api

http://localhost:8080/actuator/prometheus

> JVM & Tomcat https://docs.spring.io/spring-boot/docs/current/reference/html/actuator.html#actuator.metrics.supported

Try:

http://localhost:8080/documents 
http://localhost:8080/notfound

Refresh metrics - 

http_server_requests_seconds_count
http_server_requests_seconds_sum

```
# HELP method_timed_seconds  
# TYPE method_timed_seconds summary
method_timed_seconds_count{class="fulfilment.api.DocumentsController",exception="none",method="get",} 12.0
method_timed_seconds_sum{class="fulfilment.api.DocumentsController",exception="none",method="get",} 0.0241029
# HELP method_timed_seconds_max  
# TYPE method_timed_seconds_max gauge
method_timed_seconds_max{class="fulfilment.api.DocumentsController",exception="none",method="get",} 0.0119739
```

```
# HELP logback_events_total Number of error level events that made it to the logs
# TYPE logback_events_total counter
logback_events_total{level="warn",} 0.0
logback_events_total{level="debug",} 12.0
logback_events_total{level="error",} 0.0
logback_events_total{level="trace",} 0.0
logback_events_total{level="info",} 31.0
```

## Lab

Explore other metric types - what is the histogram