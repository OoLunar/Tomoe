FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src

COPY ./ /src
RUN dotnet restore -r linux-musl-x64 && dotnet publish -c release -r linux-musl-x64 --no-restore

FROM alpine:latest
WORKDIR /src

COPY --from=build /src/bin/release/net5.0/linux-musl-x64/publish /src
COPY ./res /src/res
RUN apk upgrade --update-cache --available && apk add openssl libstdc++ && rm -rf /var/cache/apk/*

ENTRYPOINT DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1 /src/Tomoe
