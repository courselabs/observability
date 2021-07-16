

## Reference

https://jimmybogard.com/increasing-trace-cardinality-with-tags-and-baggage/

## Custom spans

docker-compose -f labs/custom-spans/compose.yml up -d

http://localhost:8070

- get list

http://localhost:16686

- find trace

![](../img/jaeger-authz-span.png)


back to search

Service = Fulfilment.Api
Operation = database-load

what can you learn about the data store for the document fulfilment API?

- in tags, db.instance=documents and db.statement=SELECT * FROM documents


tags help with search

change user id to `1042` and try again

jaeger - search:

Service = Fulfilment.Web
Operation = authz-check
Tags: user.id=1042

> Takes you directly to authz response, with authz.allowed=False

## Baggage

docker-compose -f labs/custom-spans/update.yml up -d

list docs for 04 user

search trace - web & authz spans have transaction.id tag - this has been set in web as baggage, and copied to tags

api doesn't copy baggage to tags - but can see in logs for span
