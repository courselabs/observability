# Lab Hints

You'll need to set multiple environment variables for the output type and level, and to use structured logs.

Run your container with a name using the `--name` flag, and when it's running you can connect to a terminal session by using `docker exec -it <container_name> sh`.

Your container session will start in the application directory, and `ls` and `cat` will help you find and print the log file.

> Need more? Here's the [solution](solution.md).