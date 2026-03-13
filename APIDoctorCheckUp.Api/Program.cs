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
builder.Services.AddMonitoringEngine();
builder.Services.AddSignalRServices();

// -- Pipeline ------------------------------------------------------------------
var app = builder.Build();

app.UseOpenApiDocs();
app.UseHttpsRedirection();
app.UseCorsPolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHealthChecksEndpoint();
app.UseSignalRHubs();

app.Run();
