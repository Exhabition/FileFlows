using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using BlazorContextMenu;
using Blazored.LocalStorage;
using FileFlows.Client;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSingleton<IHotKeysService, HotKeysService>();
builder.Services.AddSingleton<INavigationService, NavigationService>();
builder.Services.AddSingleton<IClipboardService, ClipboardService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ClientService>();
builder.Services.AddBlazorContextMenu(options =>
{
    options.ConfigureTemplate(template =>
    {
        template.MenuCssClass = "context-menu";
        template.MenuItemCssClass = "context-menu-item";
        template.Animation = Animation.Grow;
    });
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
