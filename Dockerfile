FROM mcr.microsoft.com/dotnet/sdk:7.0 AS builder

WORKDIR /source
COPY src/UsbIpMonitor/UsbIpMonitor.csproj src/UsbIpMonitor/UsbIpMonitor.csproj
COPY tests/UsbIpMonitor.Tests/UsbIpMonitor.Tests.csproj tests/UsbIpMonitor.Tests/UsbIpMonitor.Tests.csproj
COPY UsbIpMonitor.sln UsbIpMonitor.sln
RUN dotnet restore

WORKDIR /source
COPY . .
RUN set -ex \
    && dotnet --version \
    && dotnet build -c Release \
    && dotnet test -c Release \
    && dotnet publish src/UsbIpMonitor/UsbIpMonitor.csproj -c Release --output /app/

FROM mcr.microsoft.com/dotnet/aspnet:7.0

LABEL maintainer "Mark Lopez <m@silvenga.com>"
LABEL org.opencontainers.image.source https://github.com/Silvenga/usbip-monitor

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "UsbIpMonitor.dll"]
