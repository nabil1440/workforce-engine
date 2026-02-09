using System.Net;
using System.Net.Http.Json;
using Workforce.Api.Contracts.Common;
using Workforce.Api.Contracts.Employees;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Employees;
using Workforce.AppCore.Services;

namespace Workforce.Api.Tests;

public sealed class EmployeesControllerTests : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient _client;

    public EmployeesControllerTests(ApiTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task List_ReturnsEmptyResult()
    {
        var response = await _client.GetAsync("/api/v1/employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PagedResponse<EmployeeResponse>>();
        Assert.NotNull(payload);
        Assert.NotNull(payload.Items);
        Assert.Empty(payload.Items);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/employees/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

internal sealed class FakeEmployeeService : IEmployeeService
{
    public Task<Result<PagedResult<Employee>>> ListAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
    {
        var result = new PagedResult<Employee>
        {
            Items = Array.Empty<Employee>(),
            TotalCount = 0,
            Page = query.Page,
            PageSize = query.PageSize
        };

        return Task.FromResult(Result<PagedResult<Employee>>.Success(result));
    }

    public Task<Result<Employee>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<Employee>.Fail(Errors.NotFound("Employee", id)));
    }

    public Task<Result<Employee>> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        employee.Id = 1;
        return Task.FromResult(Result<Employee>.Success(employee));
    }

    public Task<Result<Employee>> UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<Employee>.Success(employee));
    }

    public Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Success());
    }
}
