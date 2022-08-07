dotnet ef migrations add MessagesEntityAdded

docker run --name dev -e POSTGRES_USER=appuser -e POSTGRES_PASSWORD=Pa$$w0rd -p 5432:5432 -d postgres:latest