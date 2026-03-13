FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY APIDoctorCheckUp.slnx                                                          ./
COPY APIDoctorCheckUp.Domain/APIDoctorCheckUp.Domain.csproj                         APIDoctorCheckUp.Domain/
COPY APIDoctorCheckUp.Application/APIDoctorCheckUp.Application.csproj               APIDoctorCheckUp.Application/
COPY APIDoctorCheckUp.Infrastructure/APIDoctorCheckUp.Infrastructure.csproj         APIDoctorCheckUp.Infrastructure/
COPY APIDoctorCheckUp.Api/APIDoctorCheckUp.Api.csproj                               APIDoctorCheckUp.Api/
COPY APIDoctorCheckUp.Tests/APIDoctorCheckUp.Tests.csproj                           APIDoctorCheckUp.Tests/

RUN dotnet restore APIDoctorCheckUp.Api/APIDoctorCheckUp.Api.csproj

COPY . .

RUN dotnet publish APIDoctorCheckUp.Api/APIDoctorCheckUp.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final
WORKDIR /app

RUN mkdir -p /data && chown 1654:1654 /data

COPY --from=build --chown=1654:1654 /app/publish .

USER $APP_UID

ENV ConnectionStrings__DefaultConnection="Data Source=/data/apidoctorcheckup.db"
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "APIDoctorCheckUp.Api.dll"]
