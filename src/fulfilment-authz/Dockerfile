FROM mcr.microsoft.com/dotnet/sdk:3.1 as builder

WORKDIR /src
COPY Fulfilment.Authorization.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out --no-restore Fulfilment.Authorization.csproj

# app image
FROM mcr.microsoft.com/dotnet/aspnet:3.1

EXPOSE 80
WORKDIR /app
ENTRYPOINT ["dotnet", "Fulfilment.Authorization.dll"]

ENV Observability__Trace__Console=true \ 
    Observability__Trace__Jaeger=false \
    IdentityProvider__Url=https://blog.sixeyed.com

COPY --from=builder /out/ .