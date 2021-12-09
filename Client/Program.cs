using ChatApp.Client.Models;
using ChatApp.Client.Services;
using ChatApp.Client.ViewModel;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatApp.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddHttpClient("ChatApp.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ChatApp.ServerAPI"));

            builder.Services.AddScoped<HubUtility>();

            builder.Services.AddApiAuthorization();

            builder.Services.AddSingleton<IRoomManager, RoomManager>();

            builder.Services.AddSingleton<IndexModel>();
            builder.Services.AddTransient<IndexViewModel>();
            builder.Services.AddTransient<ChatViewModel>();

            await builder.Build().RunAsync();
        }
    }
}