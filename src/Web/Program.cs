using OfficeOpenXml;
using RoomEnglish.Infrastructure.Data;
using RoomEnglish.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Create Logs directory
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
Directory.CreateDirectory(logsPath);

// Configure built-in logging with file output
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddProvider(new FileLoggerProvider(logsPath));

// Configure EPPlus for noncommercial organizational use as required by EPPlus 8+ licensing.
ExcelPackage.License.SetNonCommercialOrganization("RoomEnglish");

// Add services to the container.
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //await app.InitialiseDatabaseAsync();  // Disabled to avoid database conflicts during development
}
else
{
    // Apply migrations on production startup with smart error handling
    await app.InitialiseDatabaseAsync();
    
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();

// Add OpenAPI/Swagger middleware BEFORE static files
app.UseOpenApi(settings =>
{
    settings.Path = "/api/specification.json";
});

app.UseStaticFiles();

// Enable CORS
app.UseCors("AllowVueApp");

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});


app.UseExceptionHandler(options => { });

// Add Authentication & Authorization middleware (REQUIRED for Google OAuth)
app.UseAuthentication();
app.UseAuthorization();

// Redirect root to API docs (temporarily - will be replaced by SPA)
// app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

// Enable SPA fallback routing - serve index.html for non-API routes
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
