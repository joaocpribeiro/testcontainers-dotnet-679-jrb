#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

#FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
#WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Issue.Tests/Issue.Tests.csproj", "Issue.Tests/"]
RUN dotnet restore "Issue.Tests/Issue.Tests.csproj"
COPY . .
#WORKDIR "/src/."
#RUN dotnet build "Issue.Tests/Issue.Tests.csproj" -c Release

#FROM build AS tests
#WORKDIR /src
#VOLUME /var/run/docker.sock:/var/run/docker.sock
#RUN dotnet test "Issue.Tests.csproj" --no-build -c Release
#RUN dotnet test --no-build -c Release --results-directory /testresults --logger "trx;LogFileName=testresults_authentication_api_it.trx" --logger=console;verbosity=detailed "Issue.Tests/Issue.Tests.csproj"
#RUN dotnet test Issue.Tests/Issue.Tests.csproj -c Release --results-directory /testresults --logger trx --logger=console;verbosity=detailed

#FROM build AS publish
#RUN dotnet publish "Issue.Tests.csproj" -c Release -o /app/publish /p:UseAppHost=false

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Issue.Tests.dll"]