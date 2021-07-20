# Lab Solution


## 1.

Browse to localhost:8080 and enter user ID 0479 - click Go; response takes 

In Jaeger search for:

- Service=Fulfilment.Web
- Operation=all
- Tags=user.id=0479

> In the trace, it's the Fulfilment.Authorization service causing the issue

In Kibana Discover, filter on:

- AppName=Fulfilment.Authorization
- UserId=0479

There's a warning log:

```
Identity provider call failed! Defaulting to: True. IDP: https://identity.sixeyed.com/authn; user ID: 0479
```

And before that an info log:

```
Using experimental identity provider for configured user. IDP: https://identity.sixeyed.com/authn; user ID: 0479
```

> Looks like we need to stick with the original identity provider


## 2. 

In Grafana, build out dashboard panels showing:

CPU:
```
sum without(instance, job) (rate(process_cpu_seconds_total{job="fulfilment-processor"}[5m]))
```

Memory:
```
dotnet_total_memory_bytes{job="fulfilment-processor"}
```

> You'll see very low numbers, so it looks like the apps are working within limits, but...

Build out an uptime panel to see if the processor is healthy:

```
sum without(job) (up{job="fulfilment-processor"})
```

> Expand the timeframe and you'll see the instances are restarting, so we'll need to dig a bit more.

In Kibana, build out a dashboard showing counts of logs:

- by log level, the sum for all apps
- filtering for errors and fatal logs, split by application name
- by application

This shows at a glance which apps are logging, the spread of log levels and which apps are logging errors.

There will be fatal logs for the fulfilment processor, so it's off to the discover tab. Filter:

- LogLevel = FATAL
- AppName = Fulfilment.Processor

You'll see log entries like this:

```
EXIT: Out of memory! Exiting immediately. Goodbye.
```

> Looks like the processors are getting saturated on memory - we need to scale up rather than out, giving the instances more memory to work with.

## 3.

This is a processing time question - which type of document takes longest to render, so would benefit most from optimization.

Start in Jaeger - search:

- Service: Fulfilment.Web
- Operation: Submit

You'll see traces taking around 1.5s to 7s. That's a pretty big range, so we should dig more.

In Prometheus check the metrics list, and you'll find a histogram called `web_document_prerender_seconds`; run a query on `web_document_prerender_seconds_bucket` and you'll see we're already collecting rendering time by document type.

A percentile would be good here: `histogram_quantile(0.90, sum without(instance, job)(rate(web_document_prerender_seconds_bucket[5m])))`

Build that into a Grafana panel and you'll see one document type has a much higher processing time.

But that's not the whole story - there's no point optimizing for PDFs unless they're commonly used.

Build a Grafana panel showing documents rendered per second: `sum without(instance, job)(rate(web_document_prerender_seconds_count[5m]))`

> PDFs take the longest time to render, but they're the least common format. DOCX is the most common, but it's also the fastest. DOC is the second most common and the second slowest, so that's probably the best bet for optimizing.