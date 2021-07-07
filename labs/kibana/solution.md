
cp -f data/fulfilment-20210707.csv labs/kibana/data/


- _Index pattern name_ = logstash-*

> See all fields

Menu: Kibana.. Discover

change index pattern - logstash-*

clear filters & search; expand timeframe to include 2021-07

click `level` field and + on ERROR

- can't filter on request ID range - not a numeric field; best you can do is search:

`message: "Request ID:" and message: 3*`

- 5 hits, but only 4 are what we want:

![](../../img/kibana-lab-solution.png)

> The wildcard search finds the additional record with error code starting with 3.