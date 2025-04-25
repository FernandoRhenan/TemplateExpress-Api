FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /src

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY ["TemplateExpress.Api/TemplateExpress.Api.csproj", "TemplateExpress.Api/"]
COPY ["TemplateExpress.Tests/TemplateExpress.Tests.csproj", "TemplateExpress.Tests/"]
RUN dotnet restore "TemplateExpress.Api/TemplateExpress.Api.csproj"
RUN dotnet restore "TemplateExpress.Tests/TemplateExpress.Tests.csproj"

COPY . .

ENV ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_USE_POLLING_FILE_WATCHER=true

ENTRYPOINT ["tail", "-f", "/dev/null"]
