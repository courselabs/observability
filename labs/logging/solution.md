# Lab Solution

You can set all the logging configuration with environment variables, and run the container in the background like this:

```
docker run -d -e Serilog__MinimumLevel=Debug -e Observability__Logging__Structured=true -e Observability__Logging__File=true -e Observability__Logging__Console=false --name processor courselabs/fulfilment-processor
```

When it's running, connect to the container with:

```
docker exec -it processor sh
```

Run `ls` and you'll see the directory contents - there's a `logs` folder in there...

```
ls -l ./logs

cat ./logs/fulfilment-processor.json
```

And you should see the log entries in JSON format. 

To bring application logs into a centralized system, you need to know the file location, log format and config options to tune logging.