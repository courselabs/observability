# Collecting logs with Logstash

## Run Logstash

```
docker-compose -f labs/logstash/compose.yml up -d

docker logs obsfun_logstash_1
```

- labs\logstash\pipeline-config\fulfilment-csv.yml
- labs\logstash\pipelines\fulfilment-csv.conf


## CSV log files

- labs\logstash\data-available\fulfilment-20210707.csv

```
# ON Windows
. .\scripts\windows-tools.ps1
```

```
cp -f data/fulfilment-20210707.csv labs/logstash/data/
```

> Takes a moment to process, then output (async)

```
docker logs obsfun_logstash_1 -f

Ctrl-C when processed
```
> Output in stdout - useful to verify configuration, CSV parsed to fields + extra metadata

```
ls labs/logstash/data/
```

> files removed after processing, because of read mode in config - useful for backload

## Logstash to Elasticsearch


- labs\logstash\pipelines\fulfilment-csv-to-es.conf
- labs\logstash\pipeline-config\heartbeat-to-es.yml

```
cp -f labs\logstash\pipeline-config\fulfilment-csv-to-es.yml labs/logstash/config/pipelines.yml
```

docker logs obsfun_logstash_1 

> `:running_pipelines=>[:"fulfilment-csv-to-es"]`

```
cp -f data/fulfilment-20210707.csv labs/logstash/data/
```

curl localhost:9200/_cat/indices?v

curl 'localhost:9200/logstash-2021.07.07/_search?q=level:ERROR&pretty' 


## Multiple log sources

- labs\logstash\pipelines\apache-to-es.conf
- labs\logstash\pipeline-config\all-to-es.yml
- data\apache_logs-small

```
cp -f labs\logstash\pipeline-config\all-to-es.yml labs/logstash/config/pipelines.yml
```

docker logs obsfun_logstash_1 

> `:running_pipelines=>[:"fulfilment-csv-to-es", :"apache-to-es"]`

cp -f data/apache_logs-small labs/logstash/data/

curl localhost:9200/_cat/indices?v=true

curl 'localhost:9200/apache-2021.06.17/_search?q=path:1983_delorean_dmc&pretty'

## lab

- malformed data - what happens?