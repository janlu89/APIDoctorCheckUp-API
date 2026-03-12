using APIDoctorCheckUp.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// -- Services ------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddOpenApiDocs();
builder.Services.AddHealthChecksConfig();

builder.Services.AddPersistence(builder.Configuration);
// Day 3: builder.Services.AddJwtAuthentication(builder.Configuration)
// Day 3: builder.Services.AddApplicationServices()
// Day 6: builder.Services.AddSignalR()

// -- Pipeline ------------------------------------------------------------------
var app = builder.Build();

app.UseOpenApiDocs();
app.UseHttpsRedirection();
app.UseCorsPolicy();       // Before auth — preflight OPTIONS requests must pass through
app.UseAuthentication();   // Validate JWT, populate HttpContext.User
app.UseAuthorization();    // Check permissions for the identified user

app.MapControllers();
app.UseHealthChecksEndpoint();

// Day 6: app.MapHub<MonitorHub>("/hubs/monitor")

app.Run();
