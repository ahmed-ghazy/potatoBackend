using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PotatoAccounting.Api.Middleware;
using PotatoAccounting.Application;
using PotatoAccounting.Infrastructure;
using PotatoAccounting.Infrastructure.Identity;
using PotatoAccounting.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "حسابات البطاطس API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Enter: Bearer {your token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
            },
            Array.Empty<string>()
        },
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    await IdentityDataSeeder.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// CORS only matters when the frontend is served from a different origin (i.e. local dev).
// In production the Angular bundle is served from the same site so the browser never sends
// a cross-origin request.
if (app.Environment.IsDevelopment())
    app.UseCors("AllowAngular");

// Serve uploaded files (images attached to entities) + the published Angular bundle.
var uploadsRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads");
Directory.CreateDirectory(uploadsRoot);
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// SPA fallback: any non-API, non-file request lands on index.html so Angular's
// client-side router can take over. Only active when an index.html is present
// (i.e. after a frontend build), keeping local API-only development unaffected.
// /api/* requests skip the fallback so an unknown endpoint still returns 404.
var webRoot = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var indexFile = Path.Combine(webRoot, "index.html");
if (File.Exists(indexFile))
{
    app.MapFallback(async ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        ctx.Response.ContentType = "text/html; charset=utf-8";
        await ctx.Response.SendFileAsync(indexFile);
    });
}

app.Run();
