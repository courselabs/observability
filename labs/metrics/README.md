
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

> JVM & Tomcat

## Lab

Explore other metric types - what is the histogram