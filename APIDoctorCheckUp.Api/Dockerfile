# -- Stage 1: Build ------------------------------------------------------------
# The SDK image contains everything needed to compile the application.
# This stage is used only during the build — it never reaches production.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first, before source code. This is a deliberate Docker
# layer caching strategy: NuGet restore is slow, but Docker only re-runs a
# layer when the files that layer depends on have changed. By copying .csproj
# files first and restoring, that restore layer is cached as long as your
# dependencies don't change — even if your source code changes constantly.
COPY APIDoctorCheckUp.sln .
COPY APIDoctorCheckUp.Domain/APIDoctorCheckUp.Domain.csproj                    APIDoctorCheckUp.Domain/
COPY APIDoctorCheckUp.Application/APIDoctorCheckUp.Application.csproj          APIDoctorCheckUp.Application/
COPY APIDoctorCheckUp.Infrastructure/APIDoctorCheckUp.Infrastructure.csproj    APIDoctorCheckUp.Infrastructure/
COPY APIDoctorCheckUp.Api/APIDoctorCheckUp.Api.csproj                          APIDoctorCheckUp.Api/
COPY APIDoctorCheckUp.Tests/APIDoctorCheckUp.Tests.csproj                      APIDoctorCheckUp.Tests/

RUN dotnet restore

# Now copy all source code and publish the Api project in Release configuration.
# --no-restore skips a redundant restore since we already did it above.
COPY . .
WORKDIR /src/APIDoctorCheckUp.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# -- Stage 2: Runtime ----------------------------------------------------------
# The aspnet runtime image is much smaller than the SDK image — no compiler,
# no CLI tooling, just what is needed to execute a compiled .NET application.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Run as a non-root user inside the container. This is a security best practice:
# if the application is ever compromised, the attacker has minimal OS privileges.
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Render.com injects a PORT environment variable at runtime.
# We default to 8080 if PORT is not set (e.g. local docker run).
ENV ASPNETCORE_URLS=http://+:\
EXPOSE 8080

ENTRYPOINT ["dotnet", "APIDoctorCheckUp.Api.dll"]
