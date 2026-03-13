using APIDoctorCheckUp.Api.Extensions;
using APIDoctorCheckUp.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// -- Services ------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddOpenApiDocs();
builder.Services.AddHealthChecksConfig();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddMonitoringEngine();
builder.Services.AddSignalRServices();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// -- Pipeline ------------------------------------------------------------------
var app = builder.Build();

app.UseExceptionHandler();
app.UseOpenApiDocs();
app.UseHttpsRedirection();
app.UseCorsPolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHealthChecksEndpoint();
app.UseSignalRHubs();

// Apply migrations before starting the host. This creates the database on first run
await app.ApplyMigrationsAsync();

app.Run();
