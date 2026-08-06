FROM --platform=$BUILDPLATFORM dhi.io/dotnet:10-sdk-alpine AS build
ARG TARGETARCH
COPY . /source
WORKDIR /source/QSM.Web
RUN ls /source
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -a "${TARGETARCH/amd64/x64}" --use-current-runtime --self-contained false -o /app

FROM dhi.io/aspnetcore:10-alpine AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "QSM.Web.dll"]
