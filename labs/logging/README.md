# Writing Logs

Logging is a boring but essential topic. Your applications need to generate useful logs to help in debugging - but not generate too many so the important information gets lost.

What you log will depend on your application, but you should ensure you log to different levels and make the level configurable. And if you use a structured logging framework, that will make searching much easier.

## Reference

- [Logging levels 101](https://stackify.com/logging-levels-101/) - typical levels and how to choose 
- [Structured logging](https://www.innoq.com/en/blog/structured-logging/) - samples in Java with Spring Boot

## Application & server logs

You'll have control over your own application logs, and you'll also use third-party components which produce their own logs.

The Apache web server produces appliction logs and access logs, recording every page visit, with request and response details.

Run Apache in a Docker container:

```
docker run -it -p 8010:80 httpd:alpine
```

> The `-it` flag means you're connecting your terminal to the container, so you'll see log entries printed out; if you see a SIGWINCH log and your container exits, that's a [known issue](https://github.com/docker-library/httpd/issues/9), you can just run the command again.

When it starts the app generates a few log lines like this:

```
[Sun Jul 18 05:22:24.034449 2021] [core:notice] [pid 1:tid 140157730523976] AH00094: Command line: 'httpd -D FOREGROUND'
```

These are application logs, using the [ErrorLogFormat](https://httpd.apache.org/docs/current/mod/core.html#errorlogformat).

📋 What do the strings `core:notice` and `AH00094` mean?

<details>
  <summary>Need some help?</summary>

`core:notice` is the component producing the log (`core`) and the logging level (`notice`). Notice logs are one level of detail higher than warning logs, and lower that informational logs.

`AH00094` is the status code of the log. Codes are useful for classifying logs, where you can use the code for all the logs for a certain event and include specific details in the log message. 

</details><br/>

This is a semi-structured log format. Each log entry is printed with extra information, but you need to know the format to make sense of it.

Apache writes a different log format to record HTTP requests.

Browse to http://localhost:8010/ and you'll see the "It works" page. Refresh a few times and then browse to a page which doesn't exist, 
http://localhost:8010/notfound.

You'll see log entries in console like this:

```
172.17.0.1 - - [18/Jul/2021:05:34:45 +0000] "GET / HTTP/1.1" 200 45
172.17.0.1 - - [18/Jul/2021:05:34:48 +0000] "GET / HTTP/1.1" 304 -
172.17.0.1 - - [18/Jul/2021:05:34:49 +0000] "GET /notfound HTTP/1.1" 404 196
```

This is another standard format - the [common log format](https://httpd.apache.org/docs/2.4/logs.html#accesslog).

📋 How could you use these logs to check your web server is running efficiently, and to keep a check on security threats?

<details>
  <summary>Need some help?</summary>

Each log includes the HTTP response code - `200` means the response was successfully sent. For pages which don't change, Apache should make use of caching. Lots of `304` responses mean the client browsers are using their local cached copies, so Apache's cache headers are working correctly.

Logs also include the source IP address. In a denial-of-service attack you could use that to identify addresses to block. You can also keep a check on `404`s - they could be automated attacks probing for known weaknesses.

</details><br/>

Run a different web server to see how the logs compare:

```
# Ctrl-C to exit Apache

docker run -it -p 8010:80 nginx:alpine
```

You'll see log entries like this:

```
/docker-entrypoint.sh: Launching /docker-entrypoint.d/30-tune-worker-processes.sh
/docker-entrypoint.sh: Configuration complete; ready for start up
```

> These are startup logs. They don't write any additional information other than the name of the startup script, and they're only written once when the app first runs.

Browse to some pages: http://localhost:8010/ and http://localhost:8010/notfound

You'll see a mixture of error and access logs, like these:

```
172.17.0.1 - - [18/Jul/2021:05:43:46 +0000] "GET / HTTP/1.1" 200 612 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:90.0) Gecko/20100101 Firefox/90.0" "-"
172.17.0.1 - - [18/Jul/2021:05:43:47 +0000] "GET / HTTP/1.1" 304 0 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:90.0) Gecko/20100101 Firefox/90.0" "-"
2021/07/18 05:43:50 [error] 32#32: *1 open() "/usr/share/nginx/html/notfound" failed (2: No such file or directory), client: 172.17.0.1, server: localhost, request: "GET /notfound HTTP/1.1", host: "localhost:8010"
172.17.0.1 - - [18/Jul/2021:05:43:50 +0000] "GET /notfound HTTP/1.1" 404 154 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:90.0) Gecko/20100101 Firefox/90.0" "-"
```

The format is different from Apache, but a lot of the same information is there - log levels and process details for the error log; URL paths, HTTP response codes and client IPs for request logs. You need to know the log format to make sense of the information.

Some servers (like Apache) let you configure the output log format, but it's your own applications where you'll have the most control.

## Log levels

Exit the Nginx container with `Ctrl-C` and run an applcation container. This is the mock document processor we've used in the metrics labs:

```
docker run -it courselabs/obsfun-fulfilment-processor
```

When it runs the app prints lots of logs - these are fake log entries for each document being processed. You'll see entries like this:

```
[05:48:48 DBG] Fulfilment request processed. Request ID: 22173649. Took: 5390ms.
[05:48:48 VRB] Fulfilment requested. Request ID: 33722562
[05:48:48 ERR] Fulfilment error! Request ID: 33722562. Error: Document service unavailable
```

📋 What do the values `DBG`, `VRB` and `ERR` mean?

<details>
  <summary>Need some help?</summary>

These are the logging levels - this app uses short codes for Debug, Verbose and Error levels.

</details><br/>

Logs are at different levels, so we can increase or reduce the number of logs generated. Document requests are logged at Verbose level - that's information you don't often need to see, so we can move to a higher level and cut those out.

This app lets you configure the logging level using an environment variable:

```
# Ctrl-C to exit

docker run -it -e Serilog__MinimumLevel=Debug courselabs/obsfun-fulfilment-processor
```

Now you'll only see Debug and Error logs. You might run at this level in test environments, or temporarily in production to track down problems. 

This app records the processing time for every request at Debug level. That can be useful to diagnose problems, but it produces a lot of logs.

📋 Stable apps will often run with minimal logging. Replace this container with one producing logs at Error level.

<details>
  <summary>Need some help?</summary>

Most apps use similar log level ranges - from Verbose (or Trace) through Information, Debug, Warning, Error and Fatal.

This app uses the `MinimumLevel` environment variable:

```
# Ctrl-C to exit

docker run -it -e Serilog__MinimumLevel=Error courselabs/obsfun-fulfilment-processor
```

</details><br/>

Now you'll only see error logs, which is probably not enough detail. It's important to use the right level for your logs, so you can run in production and record useful information to support problem diagnosis, without generating terabytes of logs.

You should also consider writing structured logs.

## Structured vs. unstructured

Unstructured and semi-structured logs are not easy to work with. You'll have a pipeline running to read logs and store them in a central database. Logs are more searchable if the key details are extracted into separate fields.

This log entry can be parsed with a regular expression, but it only has three fields you can reliably extract:

```
[05:48:48 ERR] Fulfilment error! Request ID: 33722562. Error: Document service unavailable
```

- the timestamp when the log was written
- the log level
- the log message

The log message contains a request ID and an error message, but you can only get to those by searching the whole string.

To compare that, run the document processor with a new configuration setting, so it writes structured logs:

```
# Ctrl-C to exit

docker run -it -e Observability__Logging__Structured=true courselabs/obsfun-fulfilment-processor
```

Now you'll see logs in JSON format like this:

```
{"Timestamp":"2021-07-18T06:15:44.9451895+00:00","Entry":"Fulfilment.Failed: Request ID: 31764890. Error: Document service unavailable","Level":"Error","EventType":"Fulfilment.Failed","RequestId":31764890,"ErrorMessage":"Document service unavailable","SourceContext":"Fulfilment.Processor.DocumentProcessor","MachineName":"f3b47ee9bed5","AppVersion":"1.0.0.0"}
```

JSON is much easier (and faster) to parse than a regular expression, and this entry contains a lot more information:

- a more precise timestamp
- the log message
- log level
- the type of event being logged
- the request ID
- the error message
- name of the class that wrote the message
- name of the machine the wrote the los
- version of the application

The code that writes these logs is in the [DocumentProcessor class](../../src/fulfilment-processor/DocumentProcessor.cs). It's a C# app but you don't need to be familiar with the language to see how the logs are written.

📋 Compare the logging options in the `RecordFailed` method. What is the difference with the structured logs?

<details>
  <summary>Need some help?</summary>

Here's a sample of the code:

```
if (_options.Logging.Structured)
{
    _logger.LogError("{EventType}: Request ID: {RequestId}. Error: {ErrorMessage}", EventType.Failed, requestId, errorMessage);
}
else
{
    _logger.LogError("Fulfilment error! Request ID: " + requestId +". Error: " + errorMessage);
}
```

At the time of writing the log, all the details are available as separate pieces of data - the request ID and error message. In the unstructured log those are joined into a string, but in the structured log they are preserved as separate pieces of data.

</details><br/>

Structured logs keep the key details separate from the log message, so you can extract and store them without complex, unreliable and CPU-intensive regular expression processing.

## Lab

In development your focus is on writing logs at the right level with the right details captured. At runtime you need to understand how the logging can be configured, and where the logs are being written.

For this lab your task is to run the document processor in the background (using `docker run -d` instead of `docker run -it`), writing strcutured logs at debug level to a file. Then connect to the app container and check the logs are being written to the file. 

You configure the output for this app with the setting `Observability__Logging__File=true`, but you'll need to look around to find the log file path.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___
## Cleanup

Cleanup by removing all containers:

```
docker rm -f $(docker ps -aq)
```