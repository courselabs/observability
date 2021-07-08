# Lab Solution

GET /fulfilment-json/_search?pretty&size=1
{ 
    "query": 
    {
      "bool" : {
        "must" : {
          "match" : { "ErrorMessage": "unavailable" }
        },
      "filter": [
        { "term" : { "LogLevel": "ERROR" }},
        { "term" : { "SourceContext": "Fulfilment.Processor.Program" }},
        { "term" : { "AppVersion": "1.0.0.0" }},
        { "term" : { "EventType": "Fulfilment.Failed" }},
        { "range": { "RequestId": { "gte": "30000000", "lte" : "32000000" }}}
      ]
    }
  }
}

> Same number of hits as Kibana dashboard