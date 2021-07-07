# Lab Solution


- bulk load logs

curl -H 'Content-Type: application/json' -XPOST 'localhost:9200/logs/_bulk' --data-binary '@data/logs.json'

- how many successful requests?

curl -H 'Content-Type: application/json' 'localhost:9200/logs/_search?size=0&pretty' --data-binary '@labs/elasticsearch/lab/queries/match-completed.json'

> 30 (hits.total)

- what was the request id for the one error which was not problem with doc service?

curl -H 'Content-Type: application/json' http://localhost:9200/logs/_search?pretty --data-binary '@labs/elasticsearch/lab/queries/match-error.json'

> 32441751

