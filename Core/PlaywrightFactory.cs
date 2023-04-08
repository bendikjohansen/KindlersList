using Microsoft.Playwright;

namespace Core;

public interface IPlaywrightFactory
{
    Task<IPage> NewPageAsync();
}

public class PlaywrightFactory : IPlaywrightFactory
{
    private const string UserData = "userdata";

    private IPlaywright? _playwright;
    private IBrowserContext? _browserContext;

    public async Task<IPage> NewPageAsync()
    {
        _playwright ??= await Playwright.CreateAsync();
        _browserContext ??= await _playwright.Firefox.LaunchPersistentContextAsync(UserData, new() {IgnoreHTTPSErrors = true});
        if (_browserContext == null)
        {
            throw new Exception("Could not initialize browser");
        }

        return await _browserContext.NewPageAsync();
    }
}
