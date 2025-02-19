using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;

using NotebookDB.App;
using NotebookDB.App.Services;

using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Toast;   
using BlazorSpinner;
using Blazored.SessionStorage;
using NotebookDB.Identity;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// register the cookie handler
builder.Services.AddScoped<CookieHandler>();

// set up authorization
builder.Services.AddAuthorizationCore();

// register the custom state provider
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

// register the account management interface
builder.Services.AddScoped(
    sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddScoped<API>();
builder.Services.AddScoped<SpinnerService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredSessionStorage();

// builder.Services.Configure<Microsoft.AspNetCore.Components.Forms.FileUploadOptions>(options =>
// {
//     options.MaxFileSize = 500 * 1024 * 1024; // Set maximum file size to 100 MB
// });

await builder.Build().RunAsync();


