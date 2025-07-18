# --- build ---
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    WORKDIR /src
    COPY . .
    RUN dotnet publish -c Release -o /out
    
    # --- runtime ---
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
    WORKDIR /app
    COPY --from=build /out .
    ENV ASPNETCORE_URLS=http://0.0.0.0:8080
    EXPOSE 8080
    ENTRYPOINT ["dotnet","GarageMasterBE.dll"]