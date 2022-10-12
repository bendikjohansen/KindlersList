using Microsoft.EntityFrameworkCore;

namespace Core.Extractor;

public record Extraction(int? Id, string Content, string Source, DateTime CreatedAt);

public interface IExtractionRepository
{
    Task SaveExtractions(List<Extraction> extractions);
    Task<List<Extraction>> List();
    Task<DateTime> DatePreviouslyAdded();
}

public class ExtractionRepository : IExtractionRepository
{
    private readonly IDbContextFactory<ApplicationContext> _contextFactory;

    public ExtractionRepository(IDbContextFactory<ApplicationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task SaveExtractions(List<Extraction> extractions)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await context.Extractions.AddRangeAsync(extractions);
        await context.SaveChangesAsync();
    }

    public async Task<List<Extraction>> List()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Extractions.ToListAsync();
    }

    public async Task<DateTime> DatePreviouslyAdded()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Extractions
            .Select(extraction => extraction.CreatedAt)
            .OrderBy(createdAt => createdAt)
            .LastOrDefaultAsync();
    }
}
