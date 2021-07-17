# Lab Hints

This is tricky; here are a few pointers:

- the document count is in `fulfilment_requests_total`, which is a counter - you'll need to take a rate to get the per-second increase

- you can only user operators queries if the labels on both sides match - if one set of results includes `job` and `instance`, you can't join that to another set which only includes `instance`;

- the `app_info` metric always has the value of 1. You can add labels from that metric to a set of results by multiplying and using the `group_left` function.

> Need more? Here's the [solution](solution.md).