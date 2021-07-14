
## Run Prometheus & Grafana

- metrics.yml - adds grafana setup
- labs\grafana-dashboard\config\grafana\datasource-prometheus.yml
- labs\grafana-dashboard\config\grafana\custom.ini

docker-compose -f labs/grafana-dashboard/metrics.yml up -d

http://localhost:3000

- username: admin
- password: obsfun

> Light theme

http://localhost:3000/datasources

> Prometheus is there

- labs\grafana-dashboard\config\prometheus.yml - new labels & relabel

http://localhost:9090/classic/service-discovery

`up`

## Visualzing counters and gauges

> TODO - jamboard sketch

Saturation - Memory

- dotnet 

try in prometheus: dotnet_total_memory_bytes

build into bar gauge:

- units: bytes (SI)
- thresholds: orange 3MB, red 5MB 
- legend: intance_number

![]()

- java

try jvm_memory_used_bytes

sum without(instance, job, tier, id, area) (jvm_memory_used_bytes{job="fulfilment-api"})

build into gauge

- units: bytes (SI)
- thresholds: orange 190MB, red 250MB 

- vm

(node_memory_MemTotal_bytes - node_memory_MemFree_bytes - node_memory_Buffers_bytes - node_memory_Cached_bytes - node_memory_SReclaimable_bytes) / node_memory_MemTotal_bytes 

- bar gauge
- units: percentage
- min 0, max 1.0
- horizontal

![](../../img/grafana-memory-usage.png)


CPU:

- sum without(instance, job) (rate(process_cpu_seconds_total{job="fulfilment-processor"}[5m]))
- sum without(job) (rate(process_cpu_usage{job="fulfilment-api"}[5m]))
- sum without(job, cpu, mode) (rate(node_cpu_seconds_total[5m]))

## Visualzing summaries and histograms

- java api records http_server_requests_seconds_count and http_server_requests_seconds_sum
- summary type
- use to calculate averages

 sum by(status) (rate(http_server_requests_seconds_sum{job="fulfilment-api"}[5m])) - avg processing time
 sum by(status) (rate(http_server_requests_seconds_count{job="fulfilment-api"}[5m])) - avg number reqs

 sum by(status) (rate(http_server_requests_seconds_sum{job="fulfilment-api"}[5m])) /  sum by(status) (rate(http_server_requests_seconds_count{job="fulfilment-api"}[5m])) - average duration by status

- unit: duration (s)

> coarse average, reduces data volumes but no way to get breakdowns

fulfilment_processing_seconds_bucket

- count of instances per bucket

histogram_quantile(0.90, rate(fulfilment_processing_seconds_bucket[5m]))

- 90th percentile processing times, per instance

- any aggregation needs to be done before the quantile:

histogram_quantile(0.90, sum without(instance, job, tier)(rate(fulfilment_processing_seconds_bucket[5m])))


Grafana

- histogram_quantile into time series
- avg by(le) (rate(fulfilment_processing_seconds_bucket[5m])) into bar gauge; legend={{le}}, format=heatmap


## Lab


histogram into heatmap

add network usage from https://grafana.com/grafana/dashboards/405

build full dashboard:

![](../../img/grafana-obsfun-dashboard.png)


