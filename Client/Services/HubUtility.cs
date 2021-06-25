using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Client.Services
{
    public class HubUtility
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        private readonly NavigationManager _navigationManager;

        public HubUtility(IAccessTokenProvider accessTokenProvider, NavigationManager navigationManager)
        {
            _accessTokenProvider = accessTokenProvider;
            _navigationManager = navigationManager;
        }

        public HubConnection CreateHubConnection()
        {
            return new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/chathub"), options =>
                options.AccessTokenProvider = GetAccessTokenAsync)
            .Build();
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var authTokenState = await _accessTokenProvider.RequestAccessToken();
            authTokenState.TryGetToken(out AccessToken accessToken);

            return accessToken?.Value;
        }
    }
}