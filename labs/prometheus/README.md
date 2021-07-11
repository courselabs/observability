
## Run Prometheus

- config/prometheus.yml

docker-compose -f labs/prometheus/prometheus.yml up -d

http://localhost:9090

_Status...Targets_

Run apps:

docker-compose -f labs/prometheus/apps.yml up -d

> Status, endpoints come online

_Classic UI_ - list of all metrics

- counters:
- fulfilment_requests_total
- `process_cpu_seconds_total` - 2 responses
- `system_cpu_usage` - Java

- gauges:
- `fulfilment_in_flight_total`

- text data
- `app_info`
- node_uname_info 

## Time Series Data

- fulfilment_requests_total
- fulfilment_requests_total{status="processed"}
- ^^ snapshot, latest value

- switch to Graph
- decrease time range (10m)

> Graph continually increases

fulfilment_in_flight_total - graph 

> Counter, up and down


- fulfilment_requests_total - graph
 
 > Multiple increasing lines

 ## Query API

 curl 'http://localhost:9090/api/v1/query?query=fulfilment_in_flight_total'

 - multiple metrics

curl 'http://localhost:9090/api/v1/query?query=up'

```
{"status":"success","data":{"resultType":"vector","result":[{"metric":{"__name__":"up","instance":"fulfilment-api:80","job":"fulfilment-api"},"value":[1626032997.106,"1"]},{"metric":{"__name__":"up","instance":"fulfilment-processor:9110","job":"fulfilment-processor"},"value":[1626032997.106,"1"]},{"metric":{"__name__":"up","instance":"node-exporter:9100","job":"node-exporter"},"value":[1626032997.106,"1"]}]}}
```

> Response includes timestamp from latest scraped value

Range:

curl 'http://localhost:9090/api/v1/query_range?query=fulfilment_in_flight_total&start=2021-07-11T00:01:30Z&end=2021-07-11T01:00:00Z&step=15s'

> No response, change the time period

$start=(Get-Date).AddHours(-1).ToString('yyy-MM-ddTHH:mm:ssZ') 
$end=(Get-Date).ToString('yyy-MM-ddTHH:mm:ssZ')

curl "http://localhost:9090/api/v1/query_range?query=fulfilment_in_flight_total&start=$start&end=$end&step=30s"

## Lab

build histogram graph