using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TiloiArzon.Client;
using TiloiArzon.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var configuredApiBaseUrl = builder.Configuration["ApiBaseUrl"];
var apiBaseUrl = string.IsNullOrWhiteSpace(configuredApiBaseUrl)
    ? builder.HostEnvironment.BaseAddress
    : configuredApiBaseUrl;
builder.Services.AddSingleton(new AppUrls(new Uri(apiBaseUrl)));
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ITokenStore, TokenStore>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

builder.Services.AddScoped<AuthApi>();
builder.Services.AddScoped<ProductsApi>();
builder.Services.AddScoped<CategoriesApi>();
builder.Services.AddScoped<FavoritesApi>();
builder.Services.AddScoped<CartApi>();
builder.Services.AddScoped<OrdersApi>();
builder.Services.AddScoped<AdminProductsApi>();

await builder.Build().RunAsync();
