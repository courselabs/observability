

web:

curl -H "traceparent: 00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01" localhost:8070/index

docker logs obsfun_fulfilment-web_1

activity.id starts with 00-0af7651916cd43dd8448eb211c80319c-

distributed:

curl -H "traceparent: 00-0aa1231916cd43dd8448eb211c80319c-b7ad6b7169203331-01" localhost:8070/index?all

docker logs obsfun_fulfilment-web_1

activity.id starts with 00-0aa1231916cd43dd8448eb211c80319c-

docker logs obsfun_fulfilment-api_1

span id starts with aa1231916cd43dd8448eb211c80319c:


