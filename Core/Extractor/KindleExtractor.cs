using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Core.Extractor;

public interface ISourceExtractor
{
    Task<List<Extraction>> ExtractNew();
}

public class KindleExtractor : ISourceExtractor
{
    private const string HighlightPageUrl = "https://read.amazon.com/notebook?ref_=kcr_notebook_lib&language=en-US";
    private const string LoginPageUrl = "https://www.amazon.com/ap/signin";
    private const float TimeoutMilliseconds = 10000F;

    private readonly ILogger<KindleExtractor> _logger;
    private readonly AmazonOptions _amazonOptions;
    private readonly IExtractionRepository _extractions;
    private readonly IPlaywrightFactory _playwrightFactory;

    public KindleExtractor(ILogger<KindleExtractor> logger,
        AmazonOptions amazonOptions,
        IExtractionRepository extractions,
        IPlaywrightFactory playwrightFactory)
    {
        _logger = logger;
        _amazonOptions = amazonOptions;
        _extractions = extractions;
        _playwrightFactory = playwrightFactory;
    }

    public async Task<List<Extraction>> ExtractNew()
    {
        var lastTime = await _extractions.DatePreviouslyAdded();

        var existingExtractionsTask = _extractions.List();
        var extractions = await GetExtractionsSince(lastTime);

        var existingExtractions = await existingExtractionsTask;
        var newExtractions = extractions.Where(extraction =>
                existingExtractions.All(existingExtraction => existingExtraction.Content != extraction.Content))
            .ToList();
        await _extractions.SaveExtractions(newExtractions);

        return newExtractions;
    }

    private async Task<List<Extraction>> GetExtractionsSince(DateTime startDate)
    {
        var page = await _playwrightFactory.NewPageAsync();
        page.SetDefaultTimeout(TimeoutMilliseconds);

        _logger.LogInformation("Extracting Kindle highlights after {startDate:yyyy-MM-dd}.", startDate);
        await EnsureLoggedIn(page);

        if (page.Url != HighlightPageUrl)
        {
            await page.GotoAsync(HighlightPageUrl, new() {WaitUntil = WaitUntilState.NetworkIdle});
        }

        var extractions = new List<Extraction>();
        var bookLibrary = await page.QuerySelectorAllAsync(".kp-notebook-library-each-book");

        foreach (var bookLink in bookLibrary)
        {
            await bookLink.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var extractionsForPage = await GetHighlightsFromCurrentPage(page, startDate);
            extractions.AddRange(extractionsForPage.Extractions);
            if (extractionsForPage.SkipRest)
            {
                break;
            }
        }

        _logger.LogInformation("Extracted {highlights} Kindle Highlights.", extractions.Count);
        return extractions;
    }

    private async Task<ExtractHighlightPageResult> GetHighlightsFromCurrentPage(IPage page, DateTime startDate)
    {
        var bookTitle = await page.Locator("h3").InnerTextAsync();
        var lastAccessedRaw = await page.Locator("#kp-notebook-annotated-date").InnerTextAsync();
        var lastAccessedDateOnly = lastAccessedRaw.Substring(lastAccessedRaw.IndexOf(' '));
        var lastAccessed = DateTime.Parse(lastAccessedDateOnly, CultureInfo.InvariantCulture);

        if (startDate > lastAccessed)
        {
            return new(new List<Extraction>(), true);
        }

        var texts = await page.Locator(".kp-notebook-highlight").AllInnerTextsAsync();
        var bookExtractions = texts.Select(text => new Extraction(null, text, bookTitle, DateTime.Today)).ToList();

        return new(bookExtractions, false);
    }

    private async Task EnsureLoggedIn(IPage page)
    {
        await page.GotoAsync(HighlightPageUrl, new() {WaitUntil = WaitUntilState.NetworkIdle});

        if (!page.Url.StartsWith(LoginPageUrl))
        {
            _logger.LogInformation("Already logged into Amazon...");
            return;
        }

        _logger.LogInformation("Logging into Amazon...");

        await page.Locator("#ap_email").FillAsync(_amazonOptions.Username);
        await page.Locator("#ap_password").FillAsync(_amazonOptions.Password);

        await page.Locator("#signInSubmit").ClickAsync();
        await page.WaitForTimeoutAsync(3000);
    }
}

public record ExtractHighlightPageResult(List<Extraction> Extractions, bool SkipRest);

public record AmazonOptions
{
    public const string Amazon = "Amazon";

    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
