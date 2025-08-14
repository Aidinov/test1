using DesignDocs.Api.Data;
using DesignDocs.Api.Endpoints;
using DesignDocs.Api.Repositories;
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
builder.Services.AddScoped<DiffService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapDocumentEndpoints();
app.MapReviewEndpoints();
app.MapCommentEndpoints();
app.MapTemplateEndpoints();
app.MapUserEndpoints();
app.MapGitlabWebhookEndpoints();

app.Run();

public partial class Program {}
