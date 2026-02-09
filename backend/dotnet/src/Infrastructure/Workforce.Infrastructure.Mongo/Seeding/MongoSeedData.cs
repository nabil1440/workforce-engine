using Bogus;
using MongoDB.Driver;
using Workforce.AppCore.Domain.Employees;
using Workforce.AppCore.Domain.Leaves;

namespace Workforce.Infrastructure.Mongo.Seeding;

public sealed class MongoSeedData : IMongoSeedData
{
    private readonly IMongoCollection<LeaveRequest> _collection;

    public MongoSeedData(MongoContext context)
    {
        _collection = context.LeaveRequests;
    }

    public async Task SeedLeaveRequestsAsync(IReadOnlyList<Employee> employees, CancellationToken cancellationToken = default)
    {
        if (employees.Count == 0)
        {
            return;
        }

        var existingCount = await _collection.EstimatedDocumentCountAsync(cancellationToken: cancellationToken);
        if (existingCount > 0)
        {
            return;
        }

        var faker = new Faker("en");
        var leaveRequests = new List<LeaveRequest>();

        foreach (var employee in employees)
        {
            var leaveCount = faker.Random.Int(0, 3);
            for (var index = 0; index < leaveCount; index++)
            {
                var startDateTime = faker.Date.Between(DateTime.UtcNow.AddMonths(-12), DateTime.UtcNow.AddMonths(3));
                var startDate = DateOnly.FromDateTime(startDateTime);
                var endDate = DateOnly.FromDateTime(startDateTime.AddDays(faker.Random.Int(1, 10)));
                var status = faker.PickRandom<LeaveStatus>();
                var createdAt = startDateTime.AddDays(-faker.Random.Int(1, 20));

                var leaveRequest = new LeaveRequest
                {
                    Id = Guid.NewGuid().ToString("N"),
                    EmployeeId = employee.Id,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    LeaveType = faker.PickRandom<LeaveType>(),
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status,
                    Reason = faker.Lorem.Sentence(),
                    CreatedAt = createdAt
                };

                if (status != LeaveStatus.Pending)
                {
                    leaveRequest.ApprovalHistory.Add(new LeaveRequest.ApprovalHistoryEntry
                    {
                        Status = status,
                        ChangedBy = "system",
                        ChangedAt = createdAt.AddDays(1),
                        Comment = faker.Lorem.Sentence()
                    });
                }

                leaveRequests.Add(leaveRequest);
            }
        }

        if (leaveRequests.Count == 0)
        {
            return;
        }

        await _collection.InsertManyAsync(leaveRequests, cancellationToken: cancellationToken);
    }
}
