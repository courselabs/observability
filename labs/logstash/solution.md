# Lab Solution

Copy the new Apache file to Logstash in the same way:

```
cp -f data/apache_logs-2021-05 labs/logstash/data/
```

Check the indices:

```
curl localhost:9200/_cat/indices?v
```

> You'll see several new indices starting `apache-2021.05`, and one with today's date

Query the documents for today's index, e.g.

```
curl 'localhost:9200/apache-2021.07.18/_search?size=1&pretty'
```

Logstash creates a document for every log, even if it fails processing. You'll see a document like this:

```
{
   "@version":"1",
   "host":"d22c222ad09a",
   "path":"/data/apache_logs-2021-05",
   "tags":[
      "_grokparsefailure"
   ],
   "message":"212.84.56.58 - - [21/Jly/2021:06:05:18 +0000] \"GET /projects/xdotool/xdotool.xhtml HTTP/1.1\" 200 50112 \"http://www.google.it/url?sa=t&rct=j&q=&esrc=s&source=web&cd=2&ved=0CEQQFjAB&url=http%3A%2F%2Fwww.semicomplete.com%2Fprojects%2Fxdotool%2Fxdotool.xhtml&ei=7j0BU-3qL4nB0gWP2YHQDg&usg=AFQjCNFwZFAI0RQdN_N0kFH-oj8cLsJRNQ&bvm=bv.61535280,d.d2k&cad=rja\" \"Mozilla/5.0 (X11; Ubuntu; Linux i686; rv:24.0) Gecko/20100101 Firefox/24.0\"",
   "@timestamp":"2021-07-18T20:42:26.490Z"
}
```

There's a `tags` field with the value `_grokparsefailure`, which tells you the incoming line failed to parse correctly. 

It's going to be difficult to debug that - the problem is actually the date: `21/Jly/2021` uses an invalid month name. That's enough to fail the Regular Expression so none of the other fields are extracted.