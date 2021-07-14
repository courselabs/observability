
- req per instance with version

fulfilment_requests_total * on(instance,job) group_left(app_version) app_info

- average failures per second, 5m window:

avg without(job)(rate(fulfilment_requests_total{status="failed"}[5m]))

- average processed per second, 5m window:

avg without(job)(rate(fulfilment_requests_total{status="processed"}[5m]))


- average failure rate by instance (remove status):

avg without(job, status)(rate(fulfilment_requests_total{status="failed"}[5m])) / avg without(job, status)(rate(fulfilment_requests_total{status="processed"}[5m]))


- average failure rate by instance with version:

(avg without(job, status)(rate(fulfilment_requests_total{status="failed"}[5m])) / avg without(job, status)(rate(fulfilment_requests_total{status="processed"}[5m]))) * on(instance) group_left(app_version) app_info

- average failure rate by version:

avg without(job, status, instance)(rate(fulfilment_requests_total{status="failed"}[5m]) * on(instance) group_left(app_version) app_info) / avg without(job, status, instance)(rate(fulfilment_requests_total{status="processed"}[5m])* on(instance) group_left(app_version) app_info)



| Version | Avg. Failure Rate |
|-|-|
|`{app_version="1.3.1"}`|	`0.04880891333737356`|
|`{app_version="1.5.2"}`	|`0.1361644759015841`|