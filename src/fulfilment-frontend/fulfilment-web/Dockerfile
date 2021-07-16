FROM mcr.microsoft.com/dotnet/sdk:3.1 as builder

WORKDIR /src/fulfilment-web
COPY Fulfilment.Core/Fulfilment.Core.csproj /src/Fulfilment.Core/
COPY fulfilment-web/Fulfilment.Web.csproj .
RUN dotnet restore

COPY Fulfilment.Core/. /src/Fulfilment.Core/
COPY fulfilment-web/. .
RUN dotnet publish -c Release -o /out --no-restore Fulfilment.Web.csproj

# app image
FROM mcr.microsoft.com/dotnet/aspnet:3.1

EXPOSE 80
WORKDIR /app
ENTRYPOINT ["dotnet", "Fulfilment.Web.dll"]

ENV Observability__Trace__Console="true" \ 
    Observability__Trace__Jaeger="false" \
    Observability__Trace__CustomSpans="false" \
    Observability__Trace__Baggage__Tag="false" \
    Observability__Trace__Baggage__Log="false" \
    Documents__Api__BaseUrl="http://fulfilment-api" \
    Documents__Authz__Url=""

COPY --from=builder /out/ .