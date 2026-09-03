using MemoLingo.Api.Client;
using MemoLingo.Api.Client.Contracts;
using MemoLingo.Front;
using MemoLingo.Front.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var api = builder.Configuration["MemoLingoApi"]
    ?? throw new InvalidOperationException("A configuração 'MemoLingoApi' é obrigatória.");
if (!Uri.TryCreate(api, UriKind.Absolute, out var apiUri))
    throw new InvalidOperationException("A configuração 'MemoLingoApi' deve conter uma URL absoluta válida.");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiUri });
builder.Services.AddScoped<IUsersClient>(sp => new UsersClient(api, sp.GetRequiredService<HttpClient>()));
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IWordService, WordService>();

await builder.Build().RunAsync();
