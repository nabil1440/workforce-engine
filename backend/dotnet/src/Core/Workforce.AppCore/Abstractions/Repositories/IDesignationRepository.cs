using Workforce.AppCore.Domain.Employees;

namespace Workforce.AppCore.Abstractions.Repositories;

public interface IDesignationRepository
{
    Task<IReadOnlyList<Designation>> ListAsync(CancellationToken cancellationToken = default);
}
