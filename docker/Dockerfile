FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/GroupDocs.Watermark.Mcp/GroupDocs.Watermark.Mcp.csproj \
    -c Release -o /app --no-self-contained

FROM base AS final

# Native graphics deps GroupDocs.Watermark needs on Linux for its PDF and
# Diagram content paths (which create System.Drawing.Font / Color / FontFamily
# / FontStyle instances). libgdiplus is the GDI+ implementation System.Drawing
# P/Invokes; libfontconfig1 is its hidden font-discovery dependency;
# ttf-mscorefonts-installer ships the Microsoft core fonts (Arial, Times New
# Roman, etc.) that text-watermark rendering uses by default. Pair this with
# <RuntimeHostConfigurationOption Include="System.Drawing.EnableUnixSupport"
# Value="true" /> in the consuming csproj — without all three, watermark
# rendering throws PlatformNotSupportedException or font-resolution errors on
# Linux. EULA accepted via debconf-set-selections.
USER root
RUN apt-get update \
 && echo "ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true" | debconf-set-selections \
 && apt-get install -y --no-install-recommends \
      libgdiplus libfontconfig1 ttf-mscorefonts-installer \
 && fc-cache -f -v > /dev/null \
 && rm -rf /var/lib/apt/lists/*

# OCI image labels. VERSION is supplied by the publish workflow via
# --build-arg VERSION=YY.MM.N. Default 'dev' makes local `docker build` runs
# self-identify instead of inheriting the base image's misleading version.
ARG VERSION=dev
LABEL org.opencontainers.image.title="GroupDocs.Watermark.Mcp" \
      org.opencontainers.image.description="MCP server exposing GroupDocs.Watermark for .NET as AI-callable tools (AddWatermark, AddImageWatermark, SearchWatermarks, RemoveWatermarks, GetDocumentInfo) for Claude, Cursor, GitHub Copilot, and other MCP agents." \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.source="https://github.com/groupdocs-watermark/GroupDocs.Watermark.Mcp" \
      org.opencontainers.image.url="https://www.nuget.org/packages/GroupDocs.Watermark.Mcp" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.vendor="Aspose Pty Ltd"

COPY --from=build /app .

ENV GROUPDOCS_MCP_STORAGE_PATH=/data
VOLUME /data

USER $APP_UID

ENTRYPOINT ["dotnet", "GroupDocs.Watermark.Mcp.dll"]
