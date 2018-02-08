FROM microsoft/dotnet:2.0.5-runtime
LABEL Name=bookstore.host Version=0.0.1

WORKDIR /app
COPY publish/ ./

ENTRYPOINT ["dotnet", "BookStore.Host.dll"]