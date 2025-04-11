using System.Text.Json;
using Microsoft.JSInterop;

namespace Hearts.BlazorApp.Services.LocalStorage;

public class LocalStorageService(IJSRuntime jsRuntime)
{
    private readonly IJSRuntime JSRuntime = jsRuntime;

    public async Task SetItemAsync(string key, string value)
    {
        await this.JSRuntime.InvokeVoidAsync("localStorageHelper.setItem", key, value);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        string json = JsonSerializer.Serialize(value);
        await this.JSRuntime.InvokeVoidAsync("localStorageHelper.setItem", key, json);
    }

    public async Task<string> GetItemAsync(string key)
    {
        return await this.JSRuntime.InvokeAsync<string>("localStorageHelper.getItem", key);
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        string? json = await this.JSRuntime.InvokeAsync<string>("localStorageHelper.getItem", key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task RemoveItemAsync(string key)
    {
        await this.JSRuntime.InvokeVoidAsync("localStorageHelper.removeItem", key);
    }
}
