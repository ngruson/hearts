using System.Text.Json;
using Microsoft.JSInterop;

namespace Hearts.BlazorApp.Services.LocalStorage;

public class LocalStorageService(IJSRuntime jsRuntime)
{
    private readonly IJSRuntime jsRuntime = jsRuntime;

    public async Task SetItemAsync(string key, string value)
    {
        await this.jsRuntime.InvokeVoidAsync("localStorageHelper.setItem", key, value);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        string json = JsonSerializer.Serialize(value);
        await this.jsRuntime.InvokeVoidAsync("localStorageHelper.setItem", key, json);
    }

    public async Task<string> GetItemAsync(string key)
    {
        return await this.jsRuntime.InvokeAsync<string>("localStorageHelper.getItem", key);
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        string? json = await this.jsRuntime.InvokeAsync<string>("localStorageHelper.getItem", key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task RemoveItemAsync(string key)
    {
        await this.jsRuntime.InvokeVoidAsync("localStorageHelper.removeItem", key);
    }
}
