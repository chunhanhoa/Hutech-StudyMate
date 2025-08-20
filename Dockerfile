# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .

# Thêm đoạn này để tạo file groq.key nếu có biến môi trường GROQ_API_KEY
ARG GROQ_API_KEY
RUN if [ -n "$GROQ_API_KEY" ]; then echo "$GROQ_API_KEY" > groq.key; fi

RUN dotnet publish -c Release -o /out /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /out .
# Render sẽ set PORT; nếu không có dùng 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
EXPOSE 8080
ENTRYPOINT ["dotnet","Check.dll"]
