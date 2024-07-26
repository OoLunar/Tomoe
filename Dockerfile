FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG VERSION=6.0.0
WORKDIR /src

COPY ./ /src
RUN apk add git \
    && git submodule update --init --recursive \
    && sed -i "s/<Version>.*<\/Version>/<Version>${VERSION}<\/Version>/" src/Tomoe.csproj \
    && dotnet publish -c Release -r linux-musl-x64

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
WORKDIR /src

COPY --from=build /src/src/bin/Release/net8.0/linux-musl-x64/publish /src
RUN apk upgrade --update-cache --available \
    && apk add openssl icu-libs \
    && apk del git \
    && rm -rf /var/cache/apk/*

ENTRYPOINT ["/src/Tomoe"]