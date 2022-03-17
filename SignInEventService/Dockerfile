#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["SignInEventService/SignInEventService.csproj", "SignInEventService/"]
COPY ["SignInEvent.Core/SignInEvent.Core.csproj", "SignInEvent.Core/"]
COPY ["SignInEventRepositories/SignInEventRepositories.csproj", "SignInEventRepositories/"]
RUN dotnet restore "SignInEventService/SignInEventService.csproj"
COPY . .
WORKDIR "/src/SignInEventService"
RUN dotnet build "SignInEventService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "SignInEventService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignInEventService.dll"]