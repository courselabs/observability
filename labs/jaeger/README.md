

## Run Jaeger

docker-compose -f labs/jaeger/jaeger.yml up -d

http://localhost:16686

Search & refresh 

- service=jaeger-query

_Find traces_

![](../img/jaeger-query-span.png)

Change view, top right:

- _Trace Statistics_
- _Trace JSON_

## Exporting traces to Jaeger

- apps.yml - api, web + auth service, all in app network + tracing network

docker-compose -f labs/jaeger/apps.yml up -d

http://localhost:8070

Jaeger search

- Fulfilment.Web, single trace - http GET for /

Hit Go in web UI


Jaeger search - services for all components, find for Fulfilment.Web

![](../img/jaeger-web-trace.png)

- what is the call trace?

- web to authz; authz to external; web to api
- what's taking the most time?

Fulfilment.Autz service calls https://blog.sixeyed.com; takes about 0.5 sec


## Lab

submit a doc - what is the call stack?

which method on which class actually create the document?