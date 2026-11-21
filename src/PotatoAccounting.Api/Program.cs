using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using PotatoAccounting.Api.Middleware;
using PotatoAccounting.Application;
using PotatoAccounting.Infrastructure;
using PotatoAccounting.Infrastructure.Identity;
using PotatoAccounting.Infrastructure.Persistence;
using PotatoAccounting.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Machine-local overrides (e.g. a real connection string). The file is git-ignored
// so secrets never reach the repository; it simply doesn't exist on CI.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

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

// Uploaded files live OUTSIDE wwwroot (so deployments wiping the published bundle
// don't take user data with them). We still serve them at the same /uploads/*
// URL so stored paths like "/uploads/farmers/abc.jpg" keep working.
var storageOptions = app.Services.GetRequiredService<IOptions<StorageOptions>>().Value;
var uploadsRoot = LocalFileStorage.ResolveUploadsRoot(app.Environment.ContentRootPath, storageOptions.UploadsPath);
Directory.CreateDirectory(uploadsRoot);

// One-time legacy migration: if old wwwroot/uploads still has files, move them
// to the new location. Skipped if the new location already contains files.
MigrateLegacyUploads(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads"), uploadsRoot);

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRoot),
    RequestPath = storageOptions.PublicUrlPrefix.TrimEnd('/'),
});
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

static void MigrateLegacyUploads(string legacyPath, string newPath)
{
    if (!Directory.Exists(legacyPath)) return;
    if (string.Equals(Path.GetFullPath(legacyPath), Path.GetFullPath(newPath), StringComparison.OrdinalIgnoreCase)) return;

    var legacyFiles = Directory.EnumerateFiles(legacyPath, "*", SearchOption.AllDirectories).ToList();
    if (legacyFiles.Count == 0) return;

    foreach (var src in legacyFiles)
    {
        var relative = Path.GetRelativePath(legacyPath, src);
        var dest = Path.Combine(newPath, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);

        if (File.Exists(dest)) continue; // don't overwrite if it already exists in new location
        try
        {
            File.Move(src, dest);
        }
        catch
        {
            // best-effort migration; ignore IO errors so a single bad file doesn't block startup
        }
    }
}
