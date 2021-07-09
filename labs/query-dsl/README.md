# Searching with the query DSL

## Reference

- [match query](https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl-match-query.html)
- [term query](https://www.elastic.co/guide/en/elasticsearch/reference/current/term-level-queries.html)
- [explicit mapping](https://www.elastic.co/guide/en/elasticsearch/reference/current/explicit-mapping.html)
- [42 query dsl examples](https://coralogix.com/blog/42-elasticsearch-query-examples-hands-on-tutorial/)

## Setup

Run ELK:

```
docker-compose -f labs/query-dsl/compose.yml up -d
```


Run fulfilment processor app:

```
curl -H 'Content-Type: application/json' -XPOST 'localhost:9200/logs-auto/_bulk' --data-binary "@data/logs.json"
```

> Produces logs like this:

```
2021-07-08 08:28:35.819 +00:00 [VRB] Fulfilment requested. Request ID: 31830123
```
- processor-log.yml

docker-compose -f .\labs\query-dsl\processor-log.yml up -d



- labs\query-dsl\pipelines\fulfilment-log-to-es.conf

Kibana - create index pattern `fulfilment-*`

Find a  request id; 
Cannot filter on `entry` - text field; need to search instead

## Queries and filters

```
docker stop obsfun_fulfilment-processor_1
```

In kibana console:

- term is for exact matches
GET /fulfilment-logs/_search?pretty
{ 
    "query": 
    { 
        "term":  
        {
            "entry" : 
            {
                "value": "Error: Document service unavailable"
            }
        }
    } 
}

- term is for exact matches
GET /fulfilment-logs/_search?pretty&size=1
{ 
    "query": 
    { 
        "term":  
        {
            "source" : 
            {
                "value": "fulfilment_processor"
            }
        }
    } 
}

- match for lookup; includes 302
GET /fulfilment-logs/_search?pretty&size=1
{ 
    "query": 
    { 
        "match":
        {
          "entry": "error service unavailable"
        }
    }
}

-  only service unavailable - case & punc ignored
GET /fulfilment-logs/_search?pretty&size=1
{ 
    "query": 
    { 
        "match_phrase":
        {
          "entry": "error document service unavailable"
        }
    }
}

- includes 302
GET /fulfilment-logs/_search?pretty&size=1
{ 
    "query": 
    {
      "bool" : {
        "must" : {
          "match" : { "entry": "error service unavailable" }
        },
      "filter": {
        "term" : { "level.keyword": "ERROR" }
      }
      }
    }
}

Aggregation - count of logs by level

GET /fulfilment-logs/_search?pretty&size=0
{
  "aggs": {
    "level": {
      "terms": { "field": "level.keyword" }
    }
  }
}


## Explicit mappings

GET /fulfilment-logs/_mapping?pretty

> inferred from data, all text with keyword; 

using structured data gives us more search & filter options, e.g.

```
{"Timestamp":"2021-07-08T11:18:23.1448393+01:00","Entry":"Fulfilment.Processed: Request ID: 30615888","Level":"Debug","EventType":"Fulfilment.Processed","RequestId":30615888,"SourceContext":"Fulfilment.Processor.Program","MachineName":"SC-WIN10-I7","AppVersion":"1.0.0.0"}
```

PUT /fulfilment-json
{
  "mappings": {
    "properties": {
      "LogLevel": {"type": "keyword"},
      "MachineName": {"type": "keyword"},
      "EventType": {"type": "keyword"},
      "RequestId": {"type": "integer"},
      "SourceContext": {"type": "keyword"},
      "AppVersion": {"type": "keyword"}
    }
  }
}

- processor-json.yml

docker-compose -f .\labs\query-dsl\processor-json.yml up -d


labs\query-dsl\pipelines\fulfilment-json-to-es.conf

- kibana, add index pattern just for `fulfilment-json`
- check field types; _Searchable_ and _Aggregatable_ for EventType, RequestId, Level

docker stop obsfun_fulfilment-processor_1

Filter & query in Kibana:

- LogLevel: ERROR
- SourceContext: Fulfilment.Processor.Program
- AppVersion: 1.0.0.0
- EventType: Fulfilment.Failed
- RequestId: between 30M and 32M



## Lab

Replicate dashboard in Query DSL