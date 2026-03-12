using APIDoctorCheckUp.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// -- Services ------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddOpenApiDocs();
builder.Services.AddHealthChecksConfig();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();

// Day 6: builder.Services.AddSignalR()

// -- Pipeline ------------------------------------------------------------------
var app = builder.Build();

app.UseOpenApiDocs();
app.UseHttpsRedirection();
app.UseCorsPolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHealthChecksEndpoint();

// Day 6: app.MapHub<MonitorHub>("/hubs/monitor")

app.Run();
