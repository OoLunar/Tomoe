FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG VERSION=6.0.0
WORKDIR /src

COPY ./ /src
RUN apk add git openssl icu-libs icu-data-full tzdata \
    && git submodule update --init --recursive \
    && sed -i "s/<Version>.*<\/Version>/<Version>${VERSION}<\/Version>/" src/Tomoe.csproj \
    && dotnet publish -c Release -r linux-musl-x64 \
    && rm -rf /var/cache/apk/*

ENTRYPOINT ["/src/src/bin/Release/net8.0/linux-musl-x64/publish/Tomoe"]