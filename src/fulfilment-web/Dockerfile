FROM mcr.microsoft.com/dotnet/sdk:3.1 as builder

WORKDIR /src
COPY Fulfilment.Web.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out --no-restore Fulfilment.Web.csproj

# app image
FROM mcr.microsoft.com/dotnet/aspnet:3.1

EXPOSE 80
WORKDIR /app
ENTRYPOINT ["dotnet", "Fulfilment.Web.dll"]

ENV Observability__Trace__Console=true \ 
    Observability__Trace__Jaeger=false \
    Observability__Trace__CustomSpans=false \
    Documents__Api__Url=http://fulfilment-api/documents \
    Documents__Authz__Url=http://fulfilment-authz/check

COPY --from=builder /out/ .