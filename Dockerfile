FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src

COPY ./ /src
RUN dotnet restore -r linux-musl-x64 --configfile /src/Nuget.Config
RUN dotnet publish -c release -r linux-musl-x64 --no-restore

FROM alpine:latest
WORKDIR /src
COPY --from=build /src/bin/release/net5.0/linux-musl-x64/publish /src
RUN apk upgrade --update-cache --available && apk add openssl && rm -rf /var/cache/apk/*

ENTRYPOINT ["/src/Tomoe"]
