FROM mcr.microsoft.com/playwright/dotnet:v1.26.0-focal
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN mkdir /ms-playwright-agent && cd ms-playwright-agent && \
    dotnet new Console && dotnet add package Microsoft.Playwright && dotnet build && \
    pwsh ./bin/Debug/net6.0/playwright.ps1 install && pwsh ./bin/Debug/net6.0/playwright.ps1 install-deps && \
    cd / && rm -rf /ms-playwright-agent
WORKDIR /app
COPY ./ ./
CMD ["dotnet", "run", "--project", "Console"]
