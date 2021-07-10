# Kibana Dashboards

## Reference

- [saved queries]
- [visualizations]
- [dashboards]

## Setup

Run ELK:

```
docker-compose -f labs/kibana-dashboard/logging.yml up -d
```

> Open http://localhost:5601, wait for ready


Create mapping:

PUT /fulfilment-processor
{
  "mappings": {
    "properties": {
      "LogLevel": {"type": "keyword"},
      "MachineName": {"type": "keyword"},
      "EventType": {"type": "keyword"},
      "RequestId": {"type": "integer"},
      "Duration": {"type": "integer"},
      "SourceContext": {"type": "keyword"},
      "AppVersion": {"type": "keyword"}
    }
  }
}

Run app:

```
docker-compose -f labs/kibana-dashboard/app.yml up -d
```

Kibana:

check logs written & indices are created & populated:

ls labs/kibana-dashboard/data

GET _cat/indices

- add index pattern for fulfilment logs 

`fulfilment-processor`

# Visualizations

Buckets - grouping data, e.g by log level
Metrics - aggregation over the buckets, e.g. count

Menu: _Visualize...Create_

- line
- Select fulfilment-processor index pattern
- Add bucket - x-axis; date histogram, timestamp field - split count by time range; _Update_
- Count of all logs

- Add sub-bucket- split series
- terms, loglevel

-_metrics & axes_
- change to stacked bar

![](../../img/kibana-loglevel-viz.png) 

- save, give it a sensible name

> create another visualization:

- line
- average duration - one line
- count of processed docs - second line
- only some event types record duration, so filter
- split by time
- customize axes so each line has its own y-axis

![](../../img/kibana-duration-viz.png) 

- save

## Dashboard

- create
- add existing - select loglevel chart
- add existing - select duration chart
- save

- can filter/search and select timespan, charts update

![](../../img/kibana-fulfilment-dashboard.png)

- add new visualizations:
- metric showing total count of documents processed and failed
- filetr on eventtype not in-fligt; split group by eventtype

- pie chart showing split of processing status

- markdown for headings:
```
## Fulfilment Processing

Total documents processed; processing status; production duration
```
![](![](../../img/kibana-fulfilment-dashboard-2.png))

## Lab

Export the dashboard into a JSON file

delete the Kibana data store - resets state

```
obsfun_kibana_1
```

load the dashboard into Kibana

so you can load it into a new kibana
