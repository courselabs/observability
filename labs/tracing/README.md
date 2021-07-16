

## Exporting traces

docker-compose -f labs/tracing/web.yml up -d

docker logs -f obsfun_fulfilment-web_1

http://localhost:8070

```
Activity.Id:          00-1cdb0fc1ee7a91448072c6d391106dd9-ab272d7266d63e46-01
Activity.ActivitySourceName: OpenTelemetry.Instrumentation.AspNetCore
Activity.DisplayName: /
Activity.Kind:        Server
Activity.StartTime:   2021-07-16T08:31:17.1688367Z
Activity.Duration:    00:00:00.2235717
Activity.TagObjects:
    http.host: localhost:8070
    http.method: GET
    http.path: /
    http.url: http://localhost:8070/
    http.user_agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:90.0) Gecko/20100101 Firefox/90.0
    http.status_code: 200
    otel.status_code: UNSET
Resource associated with Activity:
    service.name: Fulfilment.Web
    service.instance.id: 4b8a603f-34d3-4611-b01c-9e9c35932170
```

Hit the Go button - scroll up the logs to the last trace- why do you see the error?

```
    http.method: GET
    http.host: fulfilment-authz
    http.url: http://fulfilment-authz/check/0421/List
    otel.status_code: ERROR
    otel.status_description: Name or service not known
```

## Context propagation

Activity ID = span ID

> [Trace Context](https://www.w3.org/TR/trace-context/) - standard propagation format



docker-compose -f labs/tracing/api.yml up -d

docker logs -f obsfun_fulfilment-api_1

http://localhost:8071

http://localhost:8071/documents

```
 ** GET /documents called, in trace - id: 1520ed62cadb8966
 Span reported: 1520ed62cadb8966:1520ed62cadb8966:0:1 - get
```

Hit Go from web app

```
** GET /documents called, in trace - id: d13671b7a7735e4d8f04b97434b94b4a
Span reported: d13671b7a7735e4d8f04b97434b94b4a:50624a69efccf6a4:76885aa38b89344d:1 - get
```

ctrl-c

docker logs obsfun_fulfilment-web_1

- same trace id:

```
Activity.Id:          00-d13671b7a7735e4d8f04b97434b94b4a-76885aa38b89344d-01
Activity.ParentId:    00-d13671b7a7735e4d8f04b97434b94b4a-d306aea0276c8540-01
Activity.ActivitySourceName: OpenTelemetry.Instrumentation.Http
Activity.DisplayName: HTTP GET
Activity.Kind:        Client
Activity.StartTime:   2021-07-16T08:57:52.3731347Z
Activity.Duration:    00:00:00.0178705
Activity.TagObjects:
    http.method: GET
    http.host: fulfilment-api
    http.url: http://fulfilment-api/documents
    http.status_code: 200
    otel.status_code: UNSET
Resource associated with Activity:
    service.name: Fulfilment.Web
    service.instance.id: cf59dcaf-2878-4400-aff4-068fa3aa98f5
```

## Lab

call web with custom http header - check trace id there in web & docs

 localhost:8070/index?all
 
