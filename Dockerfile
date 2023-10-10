#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ExampleGemBox/Example.API/Example.API.csproj", "ExampleGemBox/Example.API/"]
COPY ["ExampleGemBox/Example.API.Data/Example.API.Data.csproj", "ExampleGemBox/Example.API.Data/"]
COPY ["ExampleGemBox/Example.API.Data.Contracts/Example.API.Data.Contracts.csproj", "ExampleGemBox/Example.API.Data.Contracts/"]
COPY ["ExampleGemBox/Example.API.Model/Example.API.Model.csproj", "ExampleGemBox/Example.API.Model/"]
COPY ["ExampleGemBox/Example.API.Services/Example.API.Services.csproj", "ExampleGemBox/Example.API.Services/"]
RUN dotnet restore "ExampleGemBox/Example.API/Example.API.csproj"
COPY . .
WORKDIR "/src/ExampleGemBox/Example.API"
RUN dotnet build "Example.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Example.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update
RUN apt-get install -y apt-utils
RUN apt-get install -y libgdiplus
RUN apt-get install -y libc6-dev
RUN apt-get update \
    && apt-get install -y libfontconfig1 fontconfig \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* \
    && export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/app/publish/
RUN ln -s /usr/lib/libgdiplus.so/usr/lib/gdiplus.dll
COPY --from=publish /app/publish/libSkiaSharp.so /usr/lib/
ldd libSkiaSharp.so
ENTRYPOINT ["dotnet", "Example.API.dll"
