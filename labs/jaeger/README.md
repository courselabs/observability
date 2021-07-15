
docker-compose -f labs/jaeger/jaeger.yml up -d

http://localhost:16686

Search & refresh - service=jaeger-query


docker-compose -f labs/jaeger/apps.yml up -d

