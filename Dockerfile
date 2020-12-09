# Build it
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build

WORKDIR /src

COPY ./ ./Tomoe/

RUN dotnet restore -r linux-musl-x64 ./Tomoe/Tomoe.csproj
WORKDIR /src/Tomoe
RUN dotnet build ./Tomoe.csproj -c Release -o /Tomoe/build -r linux-musl-x64

# Run it
FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine

RUN apk upgrade --update-cache --available && \
    apk add openssl && \
    rm -rf /var/cache/apk/*

WORKDIR /Tomoe
COPY --from=build /Tomoe/build .

CMD dotnet Tomoe.dll