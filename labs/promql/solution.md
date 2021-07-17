# Lab Solution


### q1 - average failures per second, 5m window:

```
avg without(job, status)(rate(fulfilment_requests_total{status="failed"}[5m]))
```

### q2 - average processed per second, 5m window:

```
avg without(job, status)(rate(fulfilment_requests_total{status="processed"}[5m]))
```

### q3 - average failure rate by instance :

```
avg without(job, status)(rate(fulfilment_requests_total{status="failed"}[5m])) / avg without(job, status)(rate(fulfilment_requests_total{status="processed"}[5m]))
```

### q4 - average failure rate by instance with version:

```
(avg without(job, status)(rate(fulfilment_requests_total{status="failed"}[5m])) / avg without(job, status)(rate(fulfilment_requests_total{status="processed"}[5m]))) * on(instance) group_left(app_version) app_info
```

### q5- average failure rate by version:

```
avg without(job, status, instance)(rate(fulfilment_requests_total{status="failed"}[5m]) * on(instance) group_left(app_version) app_info) / avg without(job, status, instance)(rate(fulfilment_requests_total{status="processed"}[5m])* on(instance) group_left(app_version) app_info)
```

### Results

You'll see output like this:

| Version | Avg. Failure Rate |
|-|-|
|`{app_version="1.3.1"}`|	`0.04880891333737356`|
|`{app_version="1.5.2"}`	|`0.1361644759015841`|

Turns out the new version is producing errors at 3X the rate of the old version. Guess we need to rollback the update :)