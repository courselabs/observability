FROM maven:3.6.3-jdk-11 AS builder

WORKDIR /usr/src/api
COPY pom.xml .
RUN mvn -B dependency:go-offline

COPY . .
RUN mvn package

# app
FROM openjdk:11-jre-slim

WORKDIR /app
COPY --from=builder /usr/src/api/target/fulfilment-api-0.1.0.jar .

EXPOSE 80
ENTRYPOINT ["java", "-jar", "/app/fulfilment-api-0.1.0.jar"]