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
    //await app.InitialiseDatabaseAsync();  // Disabled to avoid database conflicts
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable CORS
app.UseCors("AllowVueApp");

app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});


app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/api"));

app.MapEndpoints();

app.Run();

public partial class Program { }
