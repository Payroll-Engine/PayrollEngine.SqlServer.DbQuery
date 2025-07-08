FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /src

# copy solution and project files
COPY ["PayrollEngine.SqlServer.DbQuery.sln", "./"]
COPY ["DbQuery/PayrollEngine.SqlServer.DbQuery.csproj", "DbQuery/"]

# copy Directory.Build.props
COPY ["Directory.Build.props", "./"]

RUN dotnet restore "PayrollEngine.SqlServer.DbQuery.sln"

# copy everything else
COPY . .
WORKDIR "/src/DbQuery"
RUN dotnet publish "PayrollEngine.SqlServer.DbQuery.csproj" -c Release -o /app/publish --no-restore

# final stage
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=0 /app/publish .
ENTRYPOINT ["dotnet", "PayrollEngine.SqlServer.DbQuery.dll"] 