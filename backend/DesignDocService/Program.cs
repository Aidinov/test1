using Microsoft.EntityFrameworkCore;
using DesignDocService;
using DesignDocService.Endpoints;
using DesignDocService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register application services, database context and Git integration via extension
// method.  This keeps the Program.cs thin and focuses it on high-level wiring.
builder.Services.AddDesignDocServices(builder.Configuration);

// Enable controllers and API exploration (Swagger) for development convenience
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS to allow requests from any origin for demonstration purposes.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure JSON serialization to emit enums as strings so that the frontend can easily parse them.
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DesignDocContext>();
    try
    {
        context.Database.Migrate();
    }
    catch
    {
        // swallow exceptions to avoid crashing if migration fails during deployment
    }
}

// Use Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Root welcome endpoint
app.MapGet("/", () => "DesignDocService is running");

// Register API endpoints in separate modules
app.MapDocumentEndpoints();
app.MapCommentEndpoints();
app.MapProductEndpoints();
app.MapTeamEndpoints();

app.Run();

// Partial Program class definition required for integration testing via WebApplicationFactory.
public partial class Program { }