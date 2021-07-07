# Running Elasticsearch

## Reference

- [Index API](https://www.elastic.co/guide/en/elasticsearch/reference/current/indices.html)
- [Search API](https://www.elastic.co/guide/en/elasticsearch/reference/current/search-search.html)
- [Query DSL](https://www.elastic.co/guide/en/elasticsearch/reference/current/query-dsl.html)


## Running Elasticsearch in a Docker container

The standard Elasticsearch install comes with its own Java installation and an additional set of paid features which need a trial license.

We'll use a cleaner installation with a Docker container. 

- [Dockerfile]

- starts from a Docker image which has OpenJDK 11 installed
- downloads and expands the Elasticsearch 7.10 archive
- sets up the environment with a Linux user and Java options
- copies in the downloaded Elasticsearch directory & config files
- tells Docker to run the `/bin/elasticsearch` binary on startup

```
docker-compose -f labs/elasticsearch/compose.yml up -d
```

```
docker logs obsfun_elasticsearch_1 
```


_On Windows 10:_

```
. ./scripts/windows-tools.ps1
```

```
curl localhost:9200
```

What version are we running?

## Indexing documents

Indexing is how you store data in Elasticsearch. There are client libraries for all the major languages, and you can use the REST API.


```
curl -H 'Content-Type: application/json' -XPOST 'localhost:9200/logs/_doc' --data-binary "@labs/elasticsearch/data/fulfilment-requested.json"
```

> The output includes an ID you can use to retrieve the document

```
curl http://localhost:9200/logs/_doc/<id>
```

> Add `?pretty` to make the output easier to read

Add two more logs:

- file path: labs/elasticsearch/data/fulfilment-completed.json
- file path: labs/elasticsearch/data/fulfilment-errored.json

curl -H 'Content-Type: application/json' -XPOST 'localhost:9200/logs/_doc' --data-binary "@labs/elasticsearch/data/fulfilment-completed.json"

curl -H 'Content-Type: application/json' -XPOST 'localhost:9200/logs/_doc' --data-binary "@labs/elasticsearch/data/fulfilment-errored.json"

Check the details are stored:

```
curl localhost:9200/_cat/indices?v=true
```

> docs.count = 3



## Basic searching

Querystring:

curl localhost:9200/logs/_search?q=debug

curl localhost:9200/logs/_search?q=-debug&pretty

curl -H 'Content-Type: application/json' http://localhost:9200/logs/_search?pretty=true --data-binary "@labs/elasticsearch/queries/match-all.json"

DSL:

curl -H 'Content-Type: application/json' http://localhost:9200/logs/_search?pretty=true --data-binary "@labs/elasticsearch/queries/match-id.json"

curl -H 'Content-Type: application/json' http://localhost:9200/logs/_search?pretty=true --data-binary "@labs/elasticsearch/queries/match-id-level.json"


## Lab

- bulk load logs
- how many successful requests?
- what was the request id for the one error which was not problem with doc service?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).