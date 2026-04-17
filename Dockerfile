# Build stage, use a .NET machine to build this
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
# Copy entire project into docker container
COPY . ./

# Download dependencies, and compile app
# Need to indicate exactly the location of .csproj
RUN dotnet restore "src/Greenhouse.csproj"
RUN dotnet publish "src/Greenhouse.csproj" -c Release -o out

# Runtime stage, switch to a lightweight machine to run only the app
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
# Use the compiled app from build above
COPY --from=build /app/out .

EXPOSE 8080
# Run the project .csproj
CMD ["dotnet", "Greenhouse.dll"]
