# Set Up Your Lab VM

You will be provided with a Windows VM running in Azure to use for the class. The VM has all the software we'll use already installed:

- [Git](https://git-scm.com) for source control

- [Docker Desktop](https://www.docker.com/products/docker-desktop) to run apps in containers

## Connect to your VM

You'll be given the connection details for your Windows VM during the class - pick a machine from the list:

- [Lab VMs](/lab-vms.html)

You can connect to the VM using Remote Desktop on Windows, [Microsoft Remote Desktop](https://itunes.apple.com/us/app/microsoft-remote-desktop-8-0/id715768417) from the Mac App Store or [Remmina](https://github.com/FreeRDP/Remmina/wiki#for-end-users) on Linux.

_Connect to the VM. The machine name will be something like:_

```
lab001.eastus.cloudapp.azure.com
```

![](/img/setup-lab-rdp.png)

When you're connected you'll see there are icons in the taskbar to run all the apps we'll use:

![](/img/setup-lab-apps.png)

- VS Code has all the exercises ready to go; you can hit _Ctrl-`_ to open a terminal window to run commands

- PowerShell - if you prefer to have a separate terminal window

  * Be sure to switch to the lab directory: `cd /github/observability-fundamentals`

- Docker Desktop - we'll run all the demo apps in containers

## Run Docker Desktop

Click the _Docker Desktop_ icon to run Docker. You'll see a small whale notification icon next to the clock, showing Docker starting up.

> When the container animation on the whale stops, and it's fully loaded then Docker is running.

Now open PowerShell and run these commands to check Docker is running:

```
docker version
```

Your output will look like this:

```
Client:
 Cloud integration: 1.0.14
 Version:           20.10.6
 API version:       1.41
 Go version:        go1.16.3
 Git commit:        370c289
 Built:             Fri Apr  9 22:49:36 2021
 OS/Arch:           windows/amd64
 Context:           default
 Experimental:      true

Server: Docker Engine - Community
 Engine:
  Version:          20.10.6
  API version:      1.41 (minimum version 1.12)
  Go version:       go1.13.15
  Git commit:       8728dd2
  Built:            Fri Apr  9 22:44:56 2021
  OS/Arch:          linux/amd64
...
```

> Make sure you see two sets of results: `Client:` and `Server:`

And then:

```
docker-compose version
```

My output is:

```
docker-compose version 1.29.1, build c34c88b2
docker-py version: 5.0.0
CPython version: 3.9.0
OpenSSL version: OpenSSL 1.1.1g  21 Apr 2020
```

> Your details and version numbers may be different - that's fine. If you get errors then we need to look into it, because you'll need to have Docker running for all of the exercises.