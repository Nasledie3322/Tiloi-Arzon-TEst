using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using StoreAPI.Data;
using System.Text;
using System.Text.Json.Serialization;
using WebApi.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var webApiAssembly = typeof(WebApi.Services.ProductService).Assembly;
builder.Services.AddControllers()
    .AddApplicationPart(webApiAssembly)
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDataProtection()
    .SetApplicationName("TiloiArzon")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

 builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });
builder.Services.AddAuthorization();

 builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, WebApi.Services.ProductService>();
builder.Services.AddScoped<ICategoryService, WebApi.Services.CategoryService>();
builder.Services.AddScoped<IUserService, WebApi.Services.UserService>();
builder.Services.AddScoped<IOrderService, WebApi.Services.OrderService>();
builder.Services.AddScoped<IFavoriteService, WebApi.Services.FavoriteService>();
builder.Services.AddScoped<ICartService, WebApi.Services.CartService>();
builder.Services.AddScoped<ICartItemService, WebApi.Services.CartItemService>();
builder.Services.AddScoped<IAdminService, WebApi.Services.AdminService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

var hostRuntimeManifestPath = Path.Combine(AppContext.BaseDirectory, "TiloiArzon.Server.staticwebassets.runtime.json");
Dictionary<string, string>? frameworkStableToFingerprintedCache = null;
var frameworkRouteLock = new object();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/_framework", out var remainder) &&
        remainder.HasValue &&
        (string.Equals(remainder.Value, "/blazor.webassembly.js", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(remainder.Value, "/dotnet.js", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(remainder.Value, "/dotnet.runtime.js", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(remainder.Value, "/dotnet.native.js", StringComparison.OrdinalIgnoreCase)))
    {
        if (frameworkStableToFingerprintedCache == null)
        {
            lock (frameworkRouteLock)
            {
                frameworkStableToFingerprintedCache ??= TryResolveFrameworkJsRoutes(hostRuntimeManifestPath);
            }
        }

        if (frameworkStableToFingerprintedCache != null &&
            frameworkStableToFingerprintedCache.TryGetValue(context.Request.Path.Value!, out var rewritten))
        {
            context.Request.Path = "/" + rewritten;
        }
    }

    await next();
});
if (app.Environment.IsDevelopment())
{
    var nugetPackagesRoot =
        Environment.GetEnvironmentVariable("NUGET_PACKAGES") ??
        Path.Combine(
            Environment.GetEnvironmentVariable("USERPROFILE")
                ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages");

    var hotReloadPackageRoot = Path.Combine(nugetPackagesRoot, "microsoft.dotnet.hotreload.webassembly.browser");
    if (Directory.Exists(hotReloadPackageRoot))
    {
        string? latestVersionDir = null;
        try
        {
            latestVersionDir = Directory.EnumerateDirectories(hotReloadPackageRoot)
                .OrderByDescending(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }
        catch
        {
            latestVersionDir = null;
        }

        if (!string.IsNullOrWhiteSpace(latestVersionDir))
        {
            var staticWebAssetsDir = Path.Combine(latestVersionDir, "staticwebassets");
            if (Directory.Exists(staticWebAssetsDir))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(staticWebAssetsDir),
                    RequestPath = "/_content/Microsoft.DotNet.HotReload.WebAssembly.Browser"
                });
            }
        }
    }
}

 app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

 app.MapFallbackToFile("index.html");

await WebApi.Data.DbSeeder.SeedAsync(app.Services, app.Configuration);

app.Run();

static Dictionary<string, string>? TryResolveFrameworkJsRoutes(string runtimeManifestPath)
{
    if (!File.Exists(runtimeManifestPath)) return null;

    try
    {
        using var stream = File.OpenRead(runtimeManifestPath);
        using var doc = System.Text.Json.JsonDocument.Parse(stream);

        var rootChildren = doc.RootElement.GetProperty("Root").GetProperty("Children");
        if (!rootChildren.TryGetProperty("_framework", out var frameworkNode)) return null;
        if (!frameworkNode.TryGetProperty("Children", out var frameworkChildren)) return null;
        if (frameworkChildren.ValueKind != System.Text.Json.JsonValueKind.Object) return null;

        static string? findJs(System.Text.Json.JsonElement children, string prefix)
        {
            foreach (var child in children.EnumerateObject())
            {
                var name = child.Name;
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                    name.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
                    !name.EndsWith(".js.gz", StringComparison.OrdinalIgnoreCase))
                {
                    return name;
                }
            }
            return null;
        }

         
        var blazor = findJs(frameworkChildren, "blazor.webassembly.");
        var runtime = findJs(frameworkChildren, "dotnet.runtime.");
        var native = findJs(frameworkChildren, "dotnet.native.");
 
        string? dotnet = null;
        foreach (var child in frameworkChildren.EnumerateObject())
        {
            var name = child.Name;
            if (name.StartsWith("dotnet.", StringComparison.OrdinalIgnoreCase) &&
                name.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
                !name.EndsWith(".js.gz", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("dotnet.runtime.", StringComparison.OrdinalIgnoreCase) &&
                !name.StartsWith("dotnet.native.", StringComparison.OrdinalIgnoreCase))
            {
                dotnet = name;
                break;
            }
        }

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(blazor)) map["/_framework/blazor.webassembly.js"] = "_framework/" + blazor;
        if (!string.IsNullOrWhiteSpace(dotnet)) map["/_framework/dotnet.js"] = "_framework/" + dotnet;
        if (!string.IsNullOrWhiteSpace(runtime)) map["/_framework/dotnet.runtime.js"] = "_framework/" + runtime;
        if (!string.IsNullOrWhiteSpace(native)) map["/_framework/dotnet.native.js"] = "_framework/" + native;

        return map.Count == 0 ? null : map;
    }
    catch
    {
        return null;
    }
}
