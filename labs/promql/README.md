

## Labels

- instrumentation & target

docker-compose -f labs/promql/compose.yml up -d

http://localhost:9090

_Status...Targets_


Target labels:

job= fulfilment-processor
instance= fulfilment-processor-1:9110, etc.

_Graph_ + _Classic UI_

`app_info`:

| Metric | Value |
|--------|------ |
| `app_info{app_version="1.3.1",assembly_name="Fulfilment.Processor",dotnet_version="3.1.16",instance="fulfilment-processor-1:9110",job="fulfilment-processor"}` | `1` |
| `app_info{app_version="1.3.1",assembly_name="Fulfilment.Processor",dotnet_version="3.1.16",instance="fulfilment-processor-2:9110",job="fulfilment-processor"}` | `1` |
| `app_info{app_version="1.3.1",assembly_name="Fulfilment.Processor",dotnet_version="3.1.16",instance="fulfilment-processor-3:9110",job="fulfilment-processor"}` | `1` |


`app_version`, `dotnet_version` etc. are instrumentation labels

## Selection

- selection using label:

fulfilment_requests_total

- one instance:

fulfilment_requests_total{instance="fulfilment-processor-1:9110"}

- one status:

fulfilment_requests_total{status="failed"}

## Simple aggregation

Gauges are a snapshot, suitable for 

- compute success:
sum(fulfilment_requests_total{status="processed"})-sum(fulfilment_requests_total{status="failed"})

- sums:

- `sum (fulfilment_requests_total)` - aggregation operator
- sum by (instance) (fulfilment_requests_total)
- `sum without(job, instance) (fulfilment_requests_total)` - ignore other labels, sum by status

- average, max:

avg by(status) (fulfilment_requests_total)
avg by(instance) (fulfilment_requests_total{status="processed"})

- compute success:
sum(fulfilment_requests_total{status="processed"})-sum(fulfilment_requests_total{status="failed"})

- which processor has most failure:
- `max without(status, job) (fulfilment_requests_total{status="failed"})`

> no, that's the max for each processor

- `topk(1, fulfilment_requests_total{status="failed"})`


## Rates over time

Graph this:

sum without(job,status) (fulfilment_requests_total)

- always increasing; really want rate over time

fulfilment_requests_total[5m] - range vector, over last 5 minutes

rate(fulfilment_requests_total[5m]) -  avg over last 5 minutes, gauge per label set

sum without(job,status) (rate(fulfilment_requests_total[5m])) - per-second processing

sum without(job,status) (rate(fulfilment_requests_total{status="failed"}[5m])) - failed, by processor

sum(rate(fulfilment_requests_total{status="processed"}[5m]))-sum(rate(fulfilment_requests_total{status="failed"}[5m])) - success


## Staleness

docker stop promql_fulfilment-processor-1_1

> Check status page, up to 30s to find

`up` - shows -1 is down

`fulfilment_requests_total` - no data for -1, instant vector - no results, not shown

fulfilment_requests_total[5m] = -1 shows fewer than others, collection stops

`sum without(job,status) (rate(fulfilment_requests_total[5m]))` - shown, -1 trending down to 0

## Lab

group metrics - multiply together

add version number info to fulfilment requests total

fulfilment_requests_total * on(instance,job) group_left(app_version) app_info

- find the app version with the highest average failure rate:

| Version | Avg. Failure Rate |
|-|-|
|`{app_version="x"}`|	`0.05...`|
|`{app_version="y"}`	|`0.15...`|