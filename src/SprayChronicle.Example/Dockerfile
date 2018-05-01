FROM microsoft/dotnet:2.0-sdk

RUN curl -sL https://deb.nodesource.com/setup_9.x | bash - && \
    apt-get update && apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*
