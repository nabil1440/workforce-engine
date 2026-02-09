using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.Infrastructure.Mongo.Seeding;

public interface IMongoSeedData
{
    Task SeedLeaveRequestsAsync(IReadOnlyList<Employee> employees, CancellationToken cancellationToken = default);
}
