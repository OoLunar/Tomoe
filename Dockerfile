FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine
WORKDIR /src

COPY ./ /src
RUN dotnet restore -r linux-musl-x64 --configfile /src/Nuget.Config
RUN dotnet publish -c release -r linux-musl-x64 --no-restore
RUN apk upgrade --update-cache --available && apk add openssl && rm -rf /var/cache/apk/*

ENTRYPOINT ["/src/bin/release/net5.0/linux-musl-x64/publish/Tomoe"]
