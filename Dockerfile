﻿FROM mcr.microsoft.com/dotnet/sdk:7.0 AS builder

COPY . /tmp/friday

RUN dotnet restore /tmp/friday/Friday.sln
RUN dotnet build --configuration Release /tmp/friday/Friday/Friday.csproj

FROM mcr.microsoft.com/dotnet/aspnet:7.0

ENV APTLIST="nano ffmpeg"

RUN apt-get -yqq update && \
    apt-get -yqq install $APTLIST && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/* 

VOLUME ["/var/lib/friday/"]

RUN mkdir -p /bin/friday

COPY --from=builder /tmp/friday/Friday/bin/Release/net7.0/ /bin/friday/

RUN chmod +x /bin/friday/Friday

WORKDIR /var/lib/friday/

ENTRYPOINT [ "/bin/friday/Friday" ]
