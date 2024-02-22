FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ./ /src
RUN apk add git \
    && git submodule update --init --recursive \
    && dotnet publish -c Release -r linux-musl-x64

FROM alpine:latest
WORKDIR /src

COPY --from=build /src/src/bin/Release/net8.0/linux-musl-x64/publish /src
RUN apk upgrade --update-cache --available \
    && apk add openssl icu-libs \
    && apk del git \
    && rm -rf /var/cache/apk/*

ENTRYPOINT /src/Tomoe