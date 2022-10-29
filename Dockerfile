FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

COPY ./ /src
RUN dotnet publish -c Release -r linux-musl-x64 --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=embedded

FROM alpine:latest
WORKDIR /src

COPY --from=build /src/Tomoe/bin/Release/net7.0/linux-musl-x64/publish /src
COPY ./Tomoe/res /src/res
RUN apk upgrade --update-cache --available && apk add openssl libstdc++ icu-libs && rm -rf /var/cache/apk/*

ENTRYPOINT /src/Tomoe
