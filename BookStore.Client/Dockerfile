FROM microsoft/aspnetcore:latest
LABEL Name=bookstore.client Version=0.0.1

WORKDIR /app
COPY publish/ ./

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "BookStore.Client.dll"]