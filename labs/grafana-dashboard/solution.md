- avg by(le) (rate(fulfilment_processing_seconds_bucket[5m])) into heatmap; legend={{le}}, format=heatmap, panel/axes/data format=time series buckets


(node_memory_MemTotal_bytes - node_memory_MemFree_bytes - node_memory_Buffers_bytes - node_memory_Cached_bytes - node_memory_SReclaimable_bytes) / node_memory_MemTotal_bytes 


min without(mountpoint) (node_filesystem_avail_bytes) / min without(mountpoint) (node_filesystem_size_bytes)
