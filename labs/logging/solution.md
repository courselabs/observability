

```
docker run -d -e Serilog__MinimumLevel=Debug -e Observability__Logging__Structured=true -e Observability__Logging__File=true -e Observability__Logging__Console=false --name processor courselabs/obsfun-fulfilment-processor

docker exec processor ls -l /app/logs

docker exec processor cat /app/logs/fulfilment-processor.json
```

> Need to know file location, log format and config options to tune logging