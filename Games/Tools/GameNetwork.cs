
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Games.Tools;

public static class GameNetwork
{
    //static string _baseAddress = "https://poker.bsite.net/";
    static string _baseAddress = "http://poker.somee.com/";
    static HttpClient _httpClient = new();
    static string _userAgent = "Mozilla/5.0 Gecko/20210509 Firefox/69.7";


    public static async Task<string> GetValue(string param, int id)
    {
        string url = $"action=get&param={param}&id={id}";
        //_httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(_userAgent);

        //HttpResponseMessage response = await _httpClient.GetAsync($"action=get&param={param}&id={id}");
        //string content = await response.Content.ReadAsStringAsync();
        //Console.WriteLine($"{content}\n");
        //return content;

        HttpRequestMessage request = new (HttpMethod.Get, $"{_baseAddress}?{url}");
        request.SetBrowserRequestMode(BrowserRequestMode.NoCors);
        request.SetBrowserRequestCache(BrowserRequestCache.NoStore); //optional            
        bool b = request.Headers.UserAgent.TryParseAdd(_userAgent);
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        int code = (int)response.StatusCode;
        string content = await response.Content.ReadAsStringAsync();
        return content;
    }

}
