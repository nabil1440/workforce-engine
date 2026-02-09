using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Workforce.Api.Contracts.Tasks;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Employees;
using Workforce.AppCore.Domain.Projects;
using Workforce.AppCore.Services;
using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;

namespace Workforce.Api.Tests;

public sealed class TasksControllerTests : IClassFixture<TasksApiTestFactory>
{
    private readonly HttpClient _client;

    public TasksControllerTests(TasksApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetById_ReturnsAssigneeDetails()
    {
        var response = await _client.GetAsync("/api/v1/tasks/13");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(payload);
        Assert.NotNull(payload.AssignedEmployee);
        Assert.Equal(44, payload.AssignedEmployee!.Id);
        Assert.Equal("Ada Lovelace", payload.AssignedEmployee.Name);
        Assert.Equal("Engineering", payload.AssignedEmployee.Department);
        Assert.Equal("Analyst", payload.AssignedEmployee.Designation);
    }
}

public sealed class TasksApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            var values = new Dictionary<string, string?>
            {
                ["SkipMigrationsOnStartup"] = "true",
                ["SeedDataOnStartup"] = "false"
            };

            config.AddInMemoryCollection(values);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ITaskService>();
            services.RemoveAll<IEmployeeRepository>();
            services.RemoveAll<IDepartmentRepository>();
            services.RemoveAll<IDesignationRepository>();

            services.AddSingleton<ITaskService, FakeTaskService>();
            services.AddSingleton<IEmployeeRepository, FakeEmployeeRepository>();
            services.AddSingleton<IDepartmentRepository, FakeDepartmentRepository>();
            services.AddSingleton<IDesignationRepository, FakeDesignationRepository>();
        });
    }
}

internal sealed class FakeTaskService : ITaskService
{
    public Task<Result<PagedResult<WorkTask>>> ListByProjectAsync(int projectId, TaskQuery query, CancellationToken cancellationToken = default)
    {
        var result = new PagedResult<WorkTask>
        {
            Items = Array.Empty<WorkTask>(),
            TotalCount = 0,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Task.FromResult(Result<PagedResult<WorkTask>>.Success(result));
    }

    public Task<Result<WorkTask>> GetByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var task = new WorkTask
        {
            Id = taskId,
            ProjectId = 1,
            AssignedEmployeeId = 44,
            Title = "calculate card",
            Description = "Example",
            Status = ProjectTaskStatus.InProgress,
            Priority = TaskPriority.Medium,
            DueDate = new DateOnly(2025, 4, 19)
        };

        return Task.FromResult(Result<WorkTask>.Success(task));
    }

    public Task<Result<WorkTask>> CreateAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<WorkTask>.Fail(Errors.NotFound("Task", task.Id)));
    }

    public Task<Result<WorkTask>> UpdateAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<WorkTask>.Fail(Errors.NotFound("Task", task.Id)));
    }

    public Task<Result<WorkTask>> TransitionAsync(int taskId, ProjectTaskStatus toStatus, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<WorkTask>.Fail(Errors.NotFound("Task", taskId)));
    }
}

internal sealed class FakeEmployeeRepository : IEmployeeRepository
{
    public Task<PagedResult<Employee>> ListAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
    {
        var result = new PagedResult<Employee>
        {
            Items = Array.Empty<Employee>(),
            TotalCount = 0,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Task.FromResult(result);
    }

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Employee?>(null);
    }

    public Task<IReadOnlyList<Employee>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
    {
        var employee = new Employee
        {
            Id = 44,
            FirstName = "Ada",
            LastName = "Lovelace",
            DepartmentId = 1,
            DesignationId = 2,
            Email = "ada@example.com",
            IsActive = true,
            Salary = 1000,
            JoiningDate = new DateOnly(2024, 1, 1)
        };

        return Task.FromResult<IReadOnlyList<Employee>>(new[] { employee });
    }

    public Task<int> AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }

    public Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

internal sealed class FakeDepartmentRepository : IDepartmentRepository
{
    public Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Department> result = new[]
        {
            new Department { Id = 1, Name = "Engineering" }
        };

        return Task.FromResult(result);
    }
}

internal sealed class FakeDesignationRepository : IDesignationRepository
{
    public Task<IReadOnlyList<Designation>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Designation> result = new[]
        {
            new Designation { Id = 2, Name = "Analyst" }
        };

        return Task.FromResult(result);
    }
}
