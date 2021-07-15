

## Exporting traces

docker run -d -p 8070:80 --name web courselabs/obsfun-fulfilment-web

docker logs -f web

http://localhost:8070

```
Activity.Id:          |8b2bacb3-4a3f821d18a47178.
Activity.ActivitySourceName: OpenTelemetry.Instrumentation.AspNetCore
Activity.DisplayName: /
Activity.Kind:        Server
Activity.StartTime:   2021-07-14T19:31:15.2946218Z
Activity.Duration:    00:00:00.1666246
Activity.TagObjects:
    http.host: localhost:49153
    http.method: GET
    http.path: /
    http.url: http://localhost:49153/
    http.user_agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0
    http.status_code: 200
    otel.status_code: UNSET
```

> [Trace Context](https://www.w3.org/TR/trace-context/) - standard propagation format

docker run -d -p 8071:80 -e OTEL_TRACES_EXPORTER=logging --name api courselabs/obsfun-fulfilment-api

docker logs -f api

