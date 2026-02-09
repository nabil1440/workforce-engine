using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Abstractions;
using Workforce.AppCore.Domain.Employees;
using Workforce.AppCore.Services.Implementations;

namespace Workforce.AppCore.Tests;

public class EmployeeServiceTests
{
    [Fact]
    public async Task CreateAsync_FailsWhenEmployeeInvalid()
    {
        var repo = new FakeEmployeeRepository();
        var service = new EmployeeService(repo, new FakeEventPublisher());

        var employee = new Employee
        {
            LastName = "Doe",
            Email = "jane@example.com",
            DepartmentId = 1,
            DesignationId = 1,
            Salary = 1000,
            JoiningDate = new DateOnly(2024, 1, 1)
        };

        var result = await service.CreateAsync(employee);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Empty(nameof(Employee.FirstName)), result.Error);
    }

    [Fact]
    public async Task CreateAsync_SetsIdOnSuccess()
    {
        var repo = new FakeEmployeeRepository();
        var service = new EmployeeService(repo, new FakeEventPublisher());

        var employee = new Employee
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            DepartmentId = 1,
            DesignationId = 1,
            Salary = 1000,
            JoiningDate = new DateOnly(2024, 1, 1)
        };

        var result = await service.CreateAsync(employee);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound()
    {
        var repo = new FakeEmployeeRepository();
        var service = new EmployeeService(repo, new FakeEventPublisher());

        var result = await service.GetByIdAsync(10);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.NotFound("Employee", 10), result.Error);
    }

    private sealed class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly Dictionary<int, Employee> _store = new();
        private int _nextId = 1;

        public Task<PagedResult<Employee>> ListAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
        {
            var items = _store.Values.ToList();
            return Task.FromResult(new PagedResult<Employee>
            {
                Items = items,
                TotalCount = items.Count,
                Page = query.Page,
                PageSize = query.PageSize
            });
        }

        public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(id, out var employee);
            return Task.FromResult(employee);
        }

        public Task<IReadOnlyList<Employee>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
        {
            var items = ids.Select(id => _store.TryGetValue(id, out var employee) ? employee : null)
                .Where(employee => employee is not null)
                .Cast<Employee>()
                .ToList();

            return Task.FromResult<IReadOnlyList<Employee>>(items);
        }

        public Task<int> AddAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            var id = _nextId++;
            _store[id] = employee;
            return Task.FromResult(id);
        }

        public Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
        {
            _store[employee.Id] = employee;
            return Task.CompletedTask;
        }

        public Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            if (_store.TryGetValue(id, out var employee))
            {
                employee.IsActive = false;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeEventPublisher : IEventPublisher
    {
        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
