using Hearts.BlazorApp.Services.LocalStorage;

namespace Hearts.BlazorApp.Services;

public class DarkModeService(LocalStorageService localStorageService)
{
    //private bool isDarkMode;    

    public async Task<bool> GetDarkModeAsync()
    {
        bool isDarkMode = await localStorageService.GetItemAsync<bool>("darkMode");
        return isDarkMode;
    }

    public async Task SetDarkModeAsync(bool value)
    {
        bool isDarkMode = await localStorageService.GetItemAsync<bool>("darkMode");
        if (isDarkMode != value)
        {
            await localStorageService.SetItemAsync("darkMode", value);
        }
    }
}
