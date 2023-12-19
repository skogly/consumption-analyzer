using Application.Interfaces;
using Application.ValueObjects;

namespace Application.Infrastructure.Http;

public class HttpHandler : IHttpHandler
{
    private readonly HttpClient _httpClient;

    public HttpHandler(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> GetAsync<T>(Uri uri) where T : class
    {
        var response = await _httpClient.GetAsync(uri).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<T>().ConfigureAwait(false);
    }

    public async Task<T> GetAsync<T>(Url url) where T : class
    {
        var uri = new Uri(url.ToString());
        return await GetAsync<T>(uri).ConfigureAwait(false);
    }
}