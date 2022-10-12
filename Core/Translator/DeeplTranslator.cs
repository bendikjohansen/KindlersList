using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Core.Translator;

public interface ITranslator
{
    Task<List<TranslationResult>> Translate(List<string> phrases);
}

public class DeeplTranslator : ITranslator
{
    private const string DeeplUrl = "https://deepl.com";
    private const float TimeoutMilliseconds = 10000F;

    private readonly ILogger<DeeplTranslator> _logger;
    private readonly IPlaywrightFactory _playwrightFactory;

    public DeeplTranslator(ILogger<DeeplTranslator> logger, IPlaywrightFactory playwrightFactory)
    {
        _logger = logger;
        _playwrightFactory = playwrightFactory;
    }

    public async Task<List<TranslationResult>> Translate(List<string> phrases)
    {
        var page = await _playwrightFactory.NwePageAsync();
        page.SetDefaultTimeout(TimeoutMilliseconds);

        await page.GotoAsync(DeeplUrl);

        _logger.LogInformation("Translating {count} phrases.", phrases.Count);
        var translations = new List<TranslationResult>();
        foreach (var phrase in phrases)
        {
            var input = page.Locator("textarea[dl-test=\"translator-source-input\"]");
            await input.FillAsync(phrase);
            await page.WaitForTimeoutAsync(500);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var translation = await page.Locator("textarea[dl-test=\"translator-target-input\"]").InputValueAsync();
            var example = await GetExample(page);
            translations.Add(new TranslationResult(phrase, translation, example));
        }
        return translations;
    }

    private async Task<Example?> GetExample(IPage page)
    {
        var examples = page.Locator(".example");
        if (await examples.CountAsync() == 0)
        {
            return null;
        }

        var example = await examples.First.InnerTextAsync(new() {Timeout = 1000});
        var lines = example.Split('\n');
        return new Example(lines.Last(), lines.First());
    }
}

public record TranslationResult(string Original, string Translation, Example? Example);

public record Example(string Source, string Target);
