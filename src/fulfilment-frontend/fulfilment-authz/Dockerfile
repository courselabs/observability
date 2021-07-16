FROM mcr.microsoft.com/dotnet/sdk:3.1 as builder

WORKDIR /src/fulfilment-authz
COPY Fulfilment.Core/Fulfilment.Core.csproj /src/Fulfilment.Core/
COPY fulfilment-authz/Fulfilment.Authorization.csproj .
RUN dotnet restore

COPY Fulfilment.Core/. /src/Fulfilment.Core/
COPY fulfilment-authz/. .
RUN dotnet publish -c Release -o /out --no-restore Fulfilment.Authorization.csproj

# app image
FROM mcr.microsoft.com/dotnet/aspnet:3.1

EXPOSE 80
WORKDIR /app
ENTRYPOINT ["dotnet", "Fulfilment.Authorization.dll"]

ENV Observability__Trace__Console="true" \ 
    Observability__Trace__Jaeger="false" \
    Observability__Trace__CustomSpans="false" \
    Observability__Trace__Baggage__Tag="false" \
    Observability__Trace__Baggage__Log="false" \
    IdentityProvider__Url=https://blog.sixeyed.com

COPY --from=builder /out/ .