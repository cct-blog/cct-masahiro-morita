using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace blazorTest.Client.Services
{
    public static class HttpClientSelfExtentions
    {
        public static Task<HttpResponseMessage> GetWithJsonAsync<TValue>(this HttpClient client, string requestUri, TValue value)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(requestUri, UriKind.RelativeOrAbsolute));
            request.Content = JsonContent.Create(value, mediaType: null, null);
            return client.SendAsync(request, CancellationToken.None);
        }

        public static Task<HttpResponseMessage> DeleteWithJsonAsync<TValue>(this HttpClient client, string requestUri, TValue value)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, new Uri(requestUri, UriKind.RelativeOrAbsolute));
            request.Content = JsonContent.Create(value, mediaType: null, null);
            return client.SendAsync(request, CancellationToken.None);
        }
    }
}