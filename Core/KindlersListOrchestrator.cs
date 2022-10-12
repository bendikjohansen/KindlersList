using Core.Extractor;
using Core.Importer;
using Core.Translator;

namespace Core;

public interface IApplicationOrchestrator
{
    Task Synchronize();
}

public class ApplicationOrchestrator : IApplicationOrchestrator
{
    private readonly ISourceExtractor _extractor;
    private readonly ITranslator _translator;
    private readonly IImporter _importer;

    public ApplicationOrchestrator(ISourceExtractor extractor,
        ITranslator translator,
        IImporter importer)
    {
        _extractor = extractor;
        _translator = translator;
        _importer = importer;
    }

    public async Task Synchronize()
    {
        var extractions = await _extractor.ExtractNew();
        var translateQueries = extractions.Select(line => line.Content).ToList();
        var translatedLines = await _translator.Translate(translateQueries);
        var cards = translatedLines.Select(line => new Card("Deutsch", line.Original, line.Translation)).ToList();
        await _importer.Add(cards);
    }
}
