# Lab Solution

Run this query in the console:

```
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
        { "term" : { "SourceContext": "Fulfilment.Processor.DocumentProcessor" }},
        { "term" : { "AppVersion": "1.0.0.0" }},
        { "term" : { "EventType": "Fulfilment.Failed" }},
        { "range": { "RequestId": { "gte": "30000000", "lte" : "32000000" }}}
      ]
    }
  }
}
```

> You should see the same number of hits that you saw in the _Discover_ tab.