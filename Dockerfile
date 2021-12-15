FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src

COPY ./ /src
RUN dotnet restore --configfile /src/Nuget.Config -r linux-musl-x64 && dotnet publish -c Release -r linux-musl-x64 --no-restore --self-contained -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=embedded

FROM alpine:latest
WORKDIR /src

COPY --from=build /src/bin/Release/net6.0/linux-musl-x64/publish /src
COPY ./res /src/res
RUN apk upgrade --update-cache --available && apk add openssl libstdc++ icu-libs && rm -rf /var/cache/apk/*

ENTRYPOINT /src/Tomoe
