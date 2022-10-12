using Microsoft.EntityFrameworkCore;

namespace Core.Extractor;

public class ApplicationContext : DbContext
{
    public DbSet<Extraction> Extractions => Set<Extraction>();

    public ApplicationContext(DbContextOptions options) : base(options)
    {
    }
}
