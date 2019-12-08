# Compile Angular app with CLI
FROM node:13 as node-build
WORKDIR /usr/src/app

COPY TwentyQuestions.Angular/package*.json ./
RUN npm install

COPY ./TwentyQuestions.Angular/. .
RUN npm run ng build -- --prod

# Compile ASP.NET Core
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS dotnetcore-build
WORKDIR /app

# Copy csproj files and restore NuGet packages
COPY *.sln ./
COPY TwentyQuestions.Data/*.csproj ./TwentyQuestions.Data/
COPY TwentyQuestions.Database/*.sqlproj ./TwentyQuestions.Database/
COPY TwentyQuestions.Web/*.csproj ./TwentyQuestions.Web/
RUN dotnet restore

# Copy remaining code files and build
COPY ./TwentyQuestions.Data/ ./TwentyQuestions.Data/
COPY ./TwentyQuestions.Database/ ./TwentyQuestions.Database/
COPY ./TwentyQuestions.Web/ ./TwentyQuestions.Web/
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
EXPOSE 80
COPY --from=dotnetcore-build /app/out .
COPY --from=node-build /usr/src/app/dist ./wwwroot/.
ENTRYPOINT ["dotnet", "TwentyQuestions.Web.dll"]