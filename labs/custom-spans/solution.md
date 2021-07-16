


curl -H "traceparent: 00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01" -H "baggage: custom=123" localhost:8070/index


curl -H "baggage: custom.key=123" localhost:8070/index?userId=1024