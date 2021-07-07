

## Run Kibana with Elasticsearch and Logstash

```
docker-compose -f labs/kibana/compose.yml up -d

docker logs obsfun_kibana_1 -f
```

> "http server running at http://0.0.0.0:5601"

curl localhost:9200/_cat/indices?v

> Kibana creates an index to store data

Browse to http://localhost:5601

## Use the Kibana Console

> http://localhost:5601/app/dev_tools#/console

Start typing:
```
GET /_cat
```

Autocomplete. Show info about Kibana's index

```
GET /_cat/indices/.kibana?v
```

Create some new logs - a parsed log entry:

```
POST /logs-parsed/_doc
{ 
    "timestamp" : "2021-06-30T09:31:02Z", 
    "level" : "INFO",
    "source" : "766D2D343716",
    "message" : "Fulfilment completed. Request ID: 21304897"
}
```

And a structured log:

```
POST /logs-structured/_doc/docker-fun
{ 
    "timestamp" : "2021-06-30T09:31:02Z", 
    "level" : "INFO",
    "source" : "766D2D343716",
    "event_type" : "Fulfilment.Completed",
    "document_id" : 21304897,
    "message" : "Fulfilment completed. Request ID: 21304897"
}
```

Autocomplete

- GET 
- /l
- /_se
- {}
- que
- mat

```
GET /logs-structured/_search 
{
  "query": {
    "match": {
      "FIELD": "TEXT"
    }
  }
}
```

> Find all the Fulfilment.Completed events

## Searching and filtering with Kibana


Menu: Management...Stack Management

Index Patterns... Create Index Pattern

Create two index patterns:

- `logs-*` - gets documents in all the new indices
- `logs-structured` - just the structured logs

Shows fields - superset of all fields in all docs in all indices

Discover tab - no docs; expand date range to include 2021-06

Search:

- `completed`, all fields
- `message: completed`

Filter:

- `event_type` `is` - need to type a value
- `event_type.keyword` `is` - dropdown, select `Fulfilment.Completed`
- `document_id` `is between` - range, 21,000,000 and 21,500,000

## Lab

Load log data through logstash; 
Add index pattern
Filter on ERRORS; how many errors were for request IDs greater than 30,000,00?

