FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ./ /src
RUN dotnet publish -c Release -r linux-musl-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=embedded

FROM alpine:latest
WORKDIR /src

COPY --from=build /src/Tomoe/bin/Release/net8.0/linux-musl-x64/publish /src
RUN apk upgrade --update-cache --available && apk add openssl icu-libs && rm -rf /var/cache/apk/*

ENTRYPOINT /src/Tomoe