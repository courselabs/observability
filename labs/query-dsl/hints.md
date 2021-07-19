# Lab Hints

You can't add a filter clause to a straight match query, you need to use a boolean - the bool query can have a single match, and then the filter goes alongside.

The filter clause takes an array, where each element can be an exact term to match, or a range.

<details>
  <summary>You can get a head-start using this template.</summary>

```
{ 
    "query": 
    {
      "bool" : {
        "must" : {
          "match" : { <query goes here> }
        },
      "filter": [
        { "term" : { <term filter> }},
        ...
        { "range": { <range filter> }}
      ]
    }
  }
}
```
  
</details><br/>

> Need more? Here's the [solution](solution.md).