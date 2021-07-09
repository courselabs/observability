FROM mcr.microsoft.com/dotnet/sdk:3.1 as builder

WORKDIR /src
COPY Fulfilment.Processor.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out --no-restore Fulfilment.Processor.csproj

# app image
FROM mcr.microsoft.com/dotnet/runtime:3.1

WORKDIR /app
ENTRYPOINT ["dotnet", "Fulfilment.Processor.dll"]
ENV Observability__Logging__Structured=false

COPY --from=builder /out/ .