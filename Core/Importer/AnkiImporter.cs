using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Core.Importer;

public interface IImporter
{
    Task Add(List<Card> cards);
}
public class AnkiImporter : IImporter
{
    private const string LoginPageUrl = "https://ankiweb.net/account/login";
    private const string EditPageUrl = "https://ankiuser.net/edit/";

    private readonly ILogger<AnkiImporter> _logger;
    private readonly AnkiOptions _ankiOptions;
    private readonly IPlaywrightFactory _playwrightFactory;

    public AnkiImporter(ILogger<AnkiImporter> logger, AnkiOptions ankiOptions, IPlaywrightFactory playwrightFactory)
    {
        _logger = logger;
        _ankiOptions = ankiOptions;
        _playwrightFactory = playwrightFactory;
    }

    public async Task Add(List<Card> cards)
    {
        _logger.LogInformation("Adding {count} cards to Anki.", cards.Count);
        var page = await _playwrightFactory.NewPageAsync();

        await EnsureAuthenticated(page);

        await page.GotoAsync(EditPageUrl, new() {WaitUntil = WaitUntilState.NetworkIdle});

        foreach (var card in cards)
        {
            await page.Locator("#deck").SelectOptionAsync(new SelectOptionValue {Label = card.Deck});
            await page.Locator("#f0").FillAsync(card.Front);
            await page.Locator("#f1").FillAsync(card.Back);
            await page.Locator("text=Save").ClickAsync();
            await page.WaitForSelectorAsync("text=Added.");
            await page.ScreenshotAsync(new() {Path = $"./screenshots/{card.Back}.png"});
            await page.WaitForSelectorAsync("text=Added.", new () { State = WaitForSelectorState.Hidden });
        }
        await page.WaitForTimeoutAsync(2000);
    }

    private async Task EnsureAuthenticated(IPage page)
    {
        await page.GotoAsync(LoginPageUrl);
        if (page.Url == LoginPageUrl)
        {
            await page.Locator("input[name=\"username\"]").FillAsync(_ankiOptions.Username);
            await page.Locator("input[name=\"password\"]").FillAsync(_ankiOptions.Password);
            await page.Locator("input[type=\"submit\"]").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }
}

public record Card(string Deck, string Front, string Back);

public record AnkiOptions
{
    public const string Anki = "Anki";
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
