using System.Threading;
using System.Threading.Tasks;

namespace Workforce.Infrastructure.Sql.Seeding;

public interface ISqlSeedData
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
