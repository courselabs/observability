# Custom Spans

OpenTelemetry libraries should instrument incoming and outgoing HTTP requests automatically, so you get distributed tracing without any custom code.

Sometimes you want more than that - you might have CPU-intensive processing you want to record timing for, or multiple HTTP calls which you want to group and add extra details to. 

For that you can explicitly record spans in code, which can greatly add to the meaningfulness of your traces.

## Reference

- [OpenTelemetry tracing API specs](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/api.md) - useful, but very low-level detail

- [Tags, logs and baggage](https://opentracing.io/docs/overview/tags-logs-baggage/) - for recording additional detail in spans

- [Increasing Trace Cardinality with Activity Tags and Baggage](https://jimmybogard.com/increasing-trace-cardinality-with-tags-and-baggage/) - blog

## Custom spans

We'll run the same demo app as in the [Jaeger lab exercises](../jaeger/README.md):

- [compose.yml](./compose.yml) - specifies all the app containers and Jaeger; the apps are configured to record custom spans as well as the automatic HTTP request spans

Start all the containers:

```
docker-compose -f labs/custom-spans/compose.yml up -d
```

Browse to the web app at http://localhost:8070 and hit _Go_ to load the document list. 

📋 Find the trace for the document list operation - what additional spans do you see recorded?

<details>
  <summary>Need some help?</summary>

In the [Jaeger search UI](http://localhost:16686/search) find traces for all operations in the `Fulfilment.Web` service. You'll see lots, but the one you want has 10 spans recorded.

Open that trace and you'll see the same HTTP spans we've seen in previous exercises, plus three named custom spans:

- `list-documents` - an internal span used to group the whole call chain

- `authz-check` - an internal span around the authorization service call which adds the user ID, authorization type and permission response as tags

- `document-api-call` - an internal span around the document service call which adds the API action as a tag

</details><br/>

Custom spans are good for adding context to a call in your application. In the [Jaeger lab solution](../jaeger/solution.md) we guessed the user ID and authorization check from the URL the web app uses - but this custom span takes away all the guesswork:

![](../../img/jaeger-authz-span.png)

> Tags are key-value pairs which add detail to the span. They are sent to Jaeger and stored, so you can search on them. They're not propagated to other spans - they add context to one span, not the whole trace.

## Spans with tags

The code to create custom spans is straightforward, with a similar syntax across different languages. 

In C# a new span is started from an `ActivitySource` - you can see it in the `GetDocuments` method of the [ListDocumentService class](../../src/fulfilment-frontend/fulfilment-web/Services/ListDocumentsService.cs):

```
apiSpan = _activitySource.StartActivity("document-api-call");
apiSpan.AddTag("span.kind", "internal")
       .AddTag("action.type", $"{DocumentAction.List}");
```

When the span is created the API call happens inside a try block, and the span is closed in a finally block - ensuring the span is recorded even if there's a processing error.

In Java a new span is created from a `Tracer` object - you'll see this in the `get` method of the [DocumentsController class](../../src/fulfilment-api/src/main/java/com/obsfun/controllers/DocumentsController.java):

```
dbLoadSpan = tracer.buildSpan("database-load").start();
dbLoadSpan.setTag("span.kind", "internal");
...
```

Like the C# example, the processing then happens in a try block and the span is closed in a finally block.

The custom span in the Java API records some useful information about the loading process.

📋 What can you learn about the data store for the document fulfilment API from the Jaeger trace?

<details>
  <summary>Need some help?</summary>

Back in the Jaeger search, select _Service_ `Fulfilment.Api` and _Operation_
`database-load`. 

Find the traces and you'll see the trace you just closed :) You can search on span information, but the UI loads the whole trace.

Scroll down to the `database-load` span, which is the custom span that records additional tag details. Expand the tags and you'll see:

- db.type=`sql`
- db.instance=`documents`
- db.statement=`SELECT * FROM documents`

</details><br/>

Tags are not part of the _context propagation_ of the trace, so they're not included in HTTP headers, they're only sent to the trace collector. They can be used in searches which can turn your distributed tracing into a context-specific troubleshooting tool.

Browse back to the [document list page](http://localhost:8070/). Change the user ID to `1042` and hit _Go_. You'll see a `Documents service unavailable` error message, but you know the service is working for other users...

You can track this down with tags - back in the Jaeger search, select _Service_ as `Fulfilment.Web` and add a tag for the user ID: `user.id=1042`.
You'll find a single trace in the response.

📋 What's happened to the document list request for this user?

<details>
  <summary>Need some help?</summary>

The trace shows spans for the web app and the authorization service, but not the documents service - so it looks like the document list call was never made.

Expand the custom span `authz-check` and you'll see why - the tags show that this user doesn't have permissions for the list action:

- action.type=`List`
- user.id=`1042`
- authz.allowed=`False`
	
</details><br/>

> Searchable tags in spans can help you diagnose problems far more quickly than logs, if you know the initiating operation but you don't know which component caused the issue.

Tag values aren't propagated to other calls in chain - only the parent span ID (which contains the trace ID) is sent by default. But with custom spans you can attach data which does propagate through the whole call chain using _baggage_.

## Baggage in traces

Baggage can be attached to a span in key-value pairs, and it propagates through the rest of the call chain in HTTP headers. Baggage items are separate from tags and they're not shown in the Jaeger UI.

You can use baggage to send trace-wide data like a transaction ID, or even to pass additional information to a call which isn't in the service parameters.

The demo app can be configured to attach baggage to some of the custom spans:

- [update.yml](./update.yml) - turns on baggage propagation, and adds custom code to copy baggage items to tags so we do see them in Jaeger

Update the application containers:

```
docker-compose -f labs/custom-spans/update.yml up -d
```

Browse to the [document list page](http://localhost:8070/) and fetch documents for the default `0421` user ID.

📋 Find the trace for the new request. What piece of additional data gets added as baggage by the `list-documents` span?

<details>
  <summary>Need some help?</summary>

Remember this demo app copies baggage items to tags so they show up in Jaeger - that should help track it down.

The `list-documents` span has a `transaction-id` tag which isn't in the originating span, so it's been added at that point.

Check the other spans for the web app and the authorization service and you'll see the same value in their tags. Tags do not get propagated, so the data must be coming through in baggage and being copied to tags in the later spans.

Open the `get` span for the `Fulfilment.Api` service and you'll see this component doesn't copy the baggage value to a tag, but expand the logs and you'll see it in there:

- event: `baggage`
- key: `transaction.id`
- value: `f3a2a0a7-a2b3-4e5d-bddf-8d6bc0d8f1c0`

(The value is a random UUID, yours will be different).

</details><br/>

> Tags add contextual data to spans which you can use for diagnostics; baggage adds data to spans which you can use programatically in services.

You set and retrieve baggage in code in a similar way to tags. The transaction ID is set in the C# `LoadDocuments` method of the [IndexModel class](../../src/fulfilment-frontend/fulfilment-web/Pages/Index.cshtml.cs):

```
postSpan = _activitySource.StartActivity("list-documents");
postSpan.AddTag("span.kind", "internal")
        .AddTag("user.id", UserId)
        .AddBaggage("transaction.id", transactionId);
```


Baggage is a powerful option, but you need to be careful how you use it - baggage set **before** your application trace starts still gets propagated through the call tree.

## Lab

Sometimes baggage gets used for feature flags, so users can test specific workflows in the production system without any special configuration, but that can be dangerous.

The authorization service uses baggage that way - if an incoming API request has a baggage item `authz.skip=true` then the authorization check gets bypassed.

Try to force that check to get skipped, so you can list documents for an unauthorised call. Use this curl command as your starting point:

```
curl localhost:8070/index?userId=1024
```

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___
## Cleanup

Cleanup by removing all containers:

```
docker rm -f $(docker ps -aq)
```