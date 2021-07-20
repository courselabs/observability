
Problems:

1.  user 0479 is reporting slow load times

^ tracing to identify auth is slow, then logs
^ idp never responds

2. do we need to commission more fulfilment processors?

^ metrics for cpu & memory stats
^ suggests actually we can scale down
^ but check logs, resetting processor logs OOM

3. eng budget for rendering, which doc types would benefit?

^ graph processing time by doc extension
^ suggests PDF is best to improve, as rendering is slow
^ but check and PDF is least used, better to improve doc

new version of web app - is user experience better?

^ metrics for % response time
^ suggest new version is faster
^ but check status codes, returns more errors


4. 

## Start the app

Clear down:

```
docker rm -f $(docker ps -aq)
```

Observability stacks:

```
docker-compose -f hackathon/logging.yml -f hackathon/metrics.yml -f hackathon/tracing.yml up -d
```

Before running apps, create index with mappings:

```
PUT /hackathon
{
  "mappings": {
    "properties": {
      "LogLevel": {"type": "keyword"},
      "MachineName": {"type": "keyword"},
      "EventType": {"type": "keyword"},
      "RequestId": {"type": "keyword"},
      "Duration": {"type": "integer"},
      "SourceContext": {"type": "keyword"},
      "AppVersion": {"type": "keyword"},
      "AppName": {"type": "keyword"},
      "UserId": {"type": "keyword"}
    }
  }
}
```

Application stack:

```
docker-compose  -f hackathon/apps.yml up -d
```

> http://localhost:8080


Load generators:

```
docker-compose  -f hackathon/load.yml up -d
```
