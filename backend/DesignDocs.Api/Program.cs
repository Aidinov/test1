using DesignDocs.Api.Data;
using DesignDocs.Api.Endpoints;
using DesignDocs.Api.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var conn = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? "Host=localhost;Database=design_docs;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conn));

builder.Services.AddScoped<GitCliProvider>();
builder.Services.AddScoped<AnchorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/docs", async (AppDbContext db) =>
{
    return await db.Documents.ToListAsync();
});

app.MapGet("/api/docs/{id}", async (Guid id, AppDbContext db) =>
{
    return await db.Documents.FindAsync(id) is { } doc ? Results.Ok(doc) : Results.NotFound();
});

app.MapPost("/api/comments", async (Comment comment, AppDbContext db) =>
{
    db.Comments.Add(comment);
    await db.SaveChangesAsync();
    return Results.Created($"/api/comments/{comment.Id}", comment);
});

app.MapGitlabWebhookEndpoints();

app.Run();

public partial class Program {}
