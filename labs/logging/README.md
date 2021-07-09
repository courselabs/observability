

## Application & platform logs

```
docker run -it -p 8010:80 httpd:alpine
```

http://localhost:8010/
Refresh
http://localhost:8010/anyone-there

Log entries in console:

```
172.17.0.1 - - [09/Jul/2021:08:03:03 +0000] "GET / HTTP/1.1" 200 45
172.17.0.1 - - [09/Jul/2021:08:03:05 +0000] "GET / HTTP/1.1" 304 -
172.17.0.1 - - [09/Jul/2021:08:03:15 +0000] "GET /anyone-there HTTP/1.1" 404 196
```

Standard - [common log format](https://httpd.apache.org/docs/2.4/logs.html#accesslog)

```
# Ctrl-C to exit
docker run -it -p 8010:80 nginx:alpine
```

http://localhost:8010/
Refresh
http://localhost:8010/anyone-there

```
172.17.0.1 - - [09/Jul/2021:08:12:45 +0000] "GET / HTTP/1.1" 200 612 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0" "-"
172.17.0.1 - - [09/Jul/2021:08:12:50 +0000] "GET / HTTP/1.1" 304 0 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0" "-"
2021/07/09 08:12:57 [error] 32#32: *1 open() "/usr/share/nginx/html/anyone-there" failed (2: No such file or directory), client: 172.17.0.1, server: localhost, request: "GET /anyone-there HTTP/1.1", host: "localhost:8010"
172.17.0.1 - - [09/Jul/2021:08:12:57 +0000] "GET /anyone-there HTTP/1.1" 404 154 "-" "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0" "-"
```


## Log levels

```
docker run -it courselabs/obsfun-fulfilment-processor
```

```
[08:46:58 DBG] Fulfilment request processed. Request ID: 27393737
[08:46:58 VRB] Fulfilment requested. Request ID: 34446737
[08:46:58 ERR] Fulfilment error! Request ID: 34446737. Error: Document service unavailable
```

Ctrl-C to exit

docker run -it -e Serilog__MinimumLevel=Debug courselabs/obsfun-fulfilment-processor

docker run -it -e Serilog__MinimumLevel=Error courselabs/obsfun-fulfilment-processor

## Structured vs. unstructured

docker run -it -e Observability__Logging__Structured=true courselabs/obsfun-fulfilment-processor

```
{"Timestamp":"2021-07-09T08:50:45.1561319+00:00","Entry":"Fulfilment.Processed: Request ID: 34398318","Level":"Debug","EventType":"Fulfilment.Processed","RequestId":34398318,"SourceContext":"Fulfilment.Processor.Program","MachineName":"6adfb6e309a3","AppVersion":"1.0.0.0"}

{"Timestamp":"2021-07-09T08:50:45.1562692+00:00","Entry":"Fulfilment.Requested: Request ID: 32731321","Level":"Verbose","EventType":"Fulfilment.Requested","RequestId":32731321,"SourceContext":"Fulfilment.Processor.Program","MachineName":"6adfb6e309a3","AppVersion":"1.0.0.0"}

{"Timestamp":"2021-07-09T08:50:45.1563263+00:00","Entry":"Fulfilment.Failed: Request ID: 32731321. Error: Document service unavailable","Level":"Error","EventType":"Fulfilment.Failed","RequestId":32731321,"ErrorMessage":"Document service unavailable","SourceContext":"Fulfilment.Processor.Program","MachineName":"6adfb6e309a3","AppVersion":"1.0.0.0"}
```

Emit just errors:

docker run -it -e Serilog__MinimumLevel=Error -e Observability__Logging__Structured=true courselabs/obsfun-fulfilment-processor

## Writing log files

docker run -d -e Observability__Logging__File=true -e Observability__Logging__Console=false --name processor courselabs/obsfun-fulfilment-processor

docker logs processor

docker exec processor ls -l /app/logs

docker exec processor cat /app/logs/fulfilment-processor.log

docker rm -f processor

## Lab

run in background with structured logging at debug level

where are the log entries?