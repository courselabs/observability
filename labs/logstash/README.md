# Collecting Logs with Logstash

Logstash is a log processing system. You typically run it on all your servers, configured to process log files from all the apps running on the server.

Processing is done with pipelines - you specify the input file(s) to read, any parsing you want to do to the log entries, and the output destination to store logs. 

Elasticsearch is one of the output destinations, so you can run Logstash to collect all your logs and store them centrally.

## Run Logstash

Logstash is a Java application - we'll run it alongside Elasticsearch, in containers:

- [compose.yml](./compose.yml) - specifies the containers to run, loading local folders into the Logstash filesytem, which we'll use for data and configuration files.

Start Elasticsearch and Logstash, and check the Logstash logs:

```
docker-compose -f labs/logstash/compose.yml up -d

docker logs obsfun_logstash_1 -f
```

> The `-f` flag means Docker will follow the logs, watching the container and printing any new entries. 

When you see a log like this, you can exit the follow command with `Ctrl-C`:

```
[2021-07-18T19:43:19,236][INFO ][logstash.agent           ]Pipelines running {:count=>1, :running_pipelines=>[:"fulfilment-csv"], :non_running_pipelines=>[]}
```

This output says Logstash is configured with one processing pipeline, called `fulfilment-csv`. The setup for that pipeline is here:

- [pipelines/fulfilment-csv.conf](./pipelines/fulfilment-csv.conf)

📋 What do you think that pipeline is doing?

<details>
  <summary>Need some help?</summary>

There are three parts to the pipeline:

- the `input` block sets Logstash to look for files in the `/data` folder, which start with the filename `fulfilment-` and end with the extension `.csv`

- the `filter` block sets up parsing of the file, taking the input lines as comma-separated variables (CSV), and outputting named fields

- the `output` block will write all the processed log lines to the console (standard out), using a format based on the Ruby language

</details><br/>

The Logstash container is sitting and watching the data folder for any incoming CSV files.

## CSV log files

This data file has a version of the fulfilment processor logs written in the CSV format expected by the Logstash pipeline:

- [data/fulfilment-20210707.csv](../../data/fulfilment-20210707.csv) - there are 86 lines here, with a mixture of debug, info and error logs

The Docker setup mounts a local folder into the `/data` path in the Logstash container. When we copy files locally, they'll appear inside the container.

_If you're using Windows, run this script to set up a Linux-style copy command:_

```
# ON Windows - enable scripts:
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process

# then run:
. ./scripts/windows-tools.ps1
```

Now copy the CSV file into the mounted folder for the container:

```
cp -f data/fulfilment-20210707.csv labs/logstash/data/
```

> Logstash will see the file created and process it

All the log entries will be written to the container console, which is useful for checking your pipeline formatting. Follow the logs to see it:

```
docker logs obsfun_logstash_1 -f

# Ctrl-C when the processing is finished
```

📋 What is the output of the processing pipeline?

<details>
  <summary>Need some help?</summary>

Every log line has been parsed from CSV into JSON. The fields have been set using the names in the `filter` block of the pipeline, and some additional fields have been added.

This input line:

```
2021-07-07T12:28:44Z,9A687539A1D7,ERROR,Fulfilment errored! Request ID: 37868708. Error message: document service unavailable.
```

Produces this output:

```
{
      "@version" => "1",
        "source" => "9A687539A1D7",
     "timestamp" => "2021-07-07T12:28:44Z",
          "host" => "d22c222ad09a",
          "path" => "/data/fulfilment-20210707.csv",
       "message" => "Fulfilment errored! Request ID: 37868708. Error message: document service unavailable.",
    "@timestamp" => 2021-07-18T19:55:49.798Z,
         "level" => "ERROR"
}
```

</details><br/>

The output from Logstash produces semi-structured logs which are in a good format to be indexed in Elasticsearch.

## Logstash to Elasticsearch

Logstash has built-in support for Elasticsearch. This pipeline configuration is based on the previous one, but writes to Elasticsearch:

- [pipelines/fulfilment-csv-to-es.conf](./pipelines/fulfilment-csv-to-es.conf) - changes the `output` block to use Elasticsearch, with the target host name matching the Elasticsearch container name; it also extends the `filter` block to load the timestamp from the log entry

Overwrite the pipeline configuration the Logstash container is using, to load the Elasticsearch pipeline:

```
cp -f labs/logstash/pipeline-config/fulfilment-csv-to-es.yml labs/logstash/config/pipelines.yml
```

Check the logs and you'll see Logstash loading the new configuration:

```
docker logs obsfun_logstash_1 
```

> The running pipelines log should now state `:running_pipelines=>[:"fulfilment-csv-to-es"]`

📋 Copy the CSV file to the container's data directory again, and check the indices CAT API in Elasticsearch to see where the data has been loaded.

<details>
  <summary>Need some help?</summary>

You can repeat the previous copy command to reload the same CSV file:

```
cp -f data/fulfilment-20210707.csv labs/logstash/data/
```

Elasticsearch is listening on the standard port:

```
curl localhost:9200/_cat/indices?v
```

> You should see an index called `logstash-2021.07.07` with a `docs.count` of 86.

</details><br/>

Logstash loads the timestamp from the log entries in the CSV file, and stores them in an Elasticsearch index with the date in the index name.

You can query that index to find all the error logs:

```
curl 'localhost:9200/logstash-2021.07.07/_search?q=level:ERROR&pretty' 
```

> The Elasticsearch documents contain all the original data from the CSV logs, together with the extra fields set by Logstash.

So far we've just used a single pipeline with Logstash, but it can be configured to run multiple pipelines to process different logs.

## Multiple log sources

Logstash can parse CSV files and other formats - and it has built-in processing for semi-structured data with known formats. 

This pipeline parses Apache web server access logs:

- [pipelines/apache-to-es.conf](./pipelines/apache-to-es.conf)

📋 What do you think this pipeline is doing?

<details>
  <summary>Need some help?</summary>

There are the same three parts to the pipeline:

- the `input` block looks for files in the `/data` directory, starting with the name `apache_logs`

- the `filter` block sets up parsing of the file, using the `grok` processor which applies Regular Expressions. This uses a named Regular Expression for Apache logs, which is built-in to Logstash; it also extracts the timestamp from the log entry

- the `output` block writes to Elasticsearch, using an index name which begins `apache-` and ends with the date of the log entry

</details><br/>

Overwrite the Logstash pipeline configuration to load both the CSV and Apache pipelines:

```
cp -f labs/logstash/pipeline-config/all-to-es.yml labs/logstash/config/pipelines.yml
```

Check the container logs to confirm both pipelines are running:

```
docker logs obsfun_logstash_1 
```

> You should see two pipelines in the logs `:running_pipelines=>[:"fulfilment-csv-to-es", :"apache-to-es"]`

Now Logstash is watching for Apache log files as well as fulfilment processor CSV logs. 

Copy in a small sample of logs to the Logstash data directory:

```
cp -f data/apache_logs-small labs/logstash/data/
```

📋 Check the name of the Apache index in Elasticsearch. Query that index to retrieve any document - how is the data formatted?

<details>
  <summary>Need some help?</summary>

List all the indices:

```
curl localhost:9200/_cat/indices?v=true
```

> You should see an index `apache-2021.06.17` with 20 documents

You can call the search API with no query and a size of 1 to return a single document:

```
curl 'localhost:9200/apache-2021.06.17/_search?size=1&pretty'
```

The semi-structured logs have been parsed into structured JSON. 

This source:

```
83.149.9.216 - - [17/Jun/2021:10:05:43 +0000] "GET /presentations/logstash-monitorama-2013/images/kibana-dashboard3.png HTTP/1.1" 200 171717 "http://semicomplete.com/presentations/logstash-monitorama-2013/" "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1700.77 Safari/537.36"
```

Produces this output:

```
{
   "@version":"1",
   "auth":"-",
   "host":"d22c222ad09a",
   "ident":"-",
   "verb":"GET",
   "request":"/presentations/logstash-monitorama-2013/images/kibana-dashboard3.png",
   "bytes":"171717",
   "timestamp":"17/Jun/2021:10:05:43 +0000",
   "path":"/data/apache_logs-small",
   "clientip":"83.149.9.216",
   "httpversion":"1.1",
   "response":"200",
   "agent":"\"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1700.77 Safari/537.36\"",
   "referrer":"\"http://semicomplete.com/presentations/logstash-monitorama-2013/\"",
   "@timestamp":"2021-06-17T10:05:43.000Z"
}
```

> Much more useful - we can write queries for specific client IP addresses or browsers, looking for certain paths and response status codes.

</details><br/>

Logstash has lots of configuration options for parsing incoming data into structured fields. That gives you searchable logs, but the processing can be expensive - producing structured logs from your apps in the first place is a better option.


## Lab

Regular Expressions can only get you so far. Sometimes the incoming data doesn't have the format you expect, and then Logstash can't process it.

There's a much bigger Apache log file in `data/apache-logs-2021-05`. Copy that into the Logstash data directory and see what happens with the processing. 

One entry in the 5000-line log file is malformed, can you find the problem?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___
## Cleanup

Cleanup by removing all containers:

```
docker rm -f $(docker ps -aq)
```