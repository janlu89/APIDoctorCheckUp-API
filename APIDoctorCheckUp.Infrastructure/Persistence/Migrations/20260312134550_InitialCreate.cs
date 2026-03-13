using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace APIDoctorCheckUp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoredEndpoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    ExpectedStatusCode = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 200),
                    CheckIntervalSeconds = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 60),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CurrentStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoredEndpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertThresholds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EndpointId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseTimeWarningMs = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1000),
                    ResponseTimeCriticalMs = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3000),
                    ConsecutiveFailuresDown = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertThresholds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertThresholds_MonitoredEndpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "MonitoredEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EndpointId = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: true),
                    ResponseTimeMs = table.Column<long>(type: "INTEGER", nullable: false),
                    IsSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckResults_MonitoredEndpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "MonitoredEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EndpointId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TriggerReason = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_MonitoredEndpoints_EndpointId",
                        column: x => x.EndpointId,
                        principalTable: "MonitoredEndpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MonitoredEndpoints",
                columns: new[] { "Id", "CheckIntervalSeconds", "CreatedAt", "CurrentStatus", "ExpectedStatusCode", "IsActive", "Name", "Url" },
                values: new object[,]
                {
                    { 1, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unknown", 200, true, "HTTPBin GET", "https://httpbin.org/get" },
                    { 2, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unknown", 200, true, "JSONPlaceholder Post", "https://jsonplaceholder.typicode.com/posts/1" },
                    { 3, 120, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unknown", 200, true, "GitHub API", "https://api.github.com" },
                    { 4, 120, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unknown", 200, true, "Cat Fact", "https://catfact.ninja/fact" },
                    { 5, 300, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unknown", 200, true, "Snippet Vault API", "https://snippetvault.onrender.com/health" },
                    { 6, 300, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unknown", 200, true, "JSON to C# Generator API", "https://jsontocsharp.onrender.com/health" }
                });

            migrationBuilder.InsertData(
                table: "AlertThresholds",
                columns: new[] { "Id", "ConsecutiveFailuresDown", "EndpointId", "ResponseTimeCriticalMs", "ResponseTimeWarningMs" },
                values: new object[,]
                {
                    { 1, 3, 1, 3000, 1000 },
                    { 2, 3, 2, 3000, 1000 },
                    { 3, 3, 3, 3000, 1000 },
                    { 4, 3, 4, 3000, 1000 },
                    { 5, 3, 5, 3000, 1000 },
                    { 6, 3, 6, 3000, 1000 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertThresholds_EndpointId",
                table: "AlertThresholds",
                column: "EndpointId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckResults_EndpointId_CheckedAt",
                table: "CheckResults",
                columns: new[] { "EndpointId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_EndpointId_ResolvedAt",
                table: "Incidents",
                columns: new[] { "EndpointId", "ResolvedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertThresholds");

            migrationBuilder.DropTable(
                name: "CheckResults");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "MonitoredEndpoints");
        }
    }
}
