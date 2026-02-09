using Bogus;
using Microsoft.EntityFrameworkCore;
using Workforce.AppCore.Domain.Employees;
using Workforce.AppCore.Domain.Projects;
using TaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;

namespace Workforce.Infrastructure.Sql.Seeding;

public sealed class SqlSeedData : ISqlSeedData
{
    private const int EmployeeCount = 50;
    private const int ProjectCount = 8;

    private static readonly string[] DepartmentNames =
    [
        "Engineering",
        "Product",
        "Design",
        "People",
        "Finance",
        "Sales",
        "Marketing",
        "Operations"
    ];

    private static readonly string[] DesignationNames =
    [
        "Intern",
        "Associate",
        "Senior",
        "Lead",
        "Manager",
        "Director"
    ];

    private readonly WorkforceDbContext _dbContext;

    public SqlSeedData(WorkforceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Employees.AnyAsync(cancellationToken))
        {
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var departments = DepartmentNames
            .Select(name => new Department { Name = name })
            .ToList();
        var designations = DesignationNames
            .Select(name => new Designation { Name = name })
            .ToList();

        _dbContext.Departments.AddRange(departments);
        _dbContext.Designations.AddRange(designations);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var employeeFaker = new Faker<Employee>("en")
            .RuleFor(e => e.FirstName, f => f.Name.FirstName())
            .RuleFor(e => e.LastName, f => f.Name.LastName())
            .RuleFor(e => e.Email, (f, e) =>
                f.Internet.Email(e.FirstName, e.LastName, "example.com", f.UniqueIndex.ToString()))
            .RuleFor(e => e.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(e => e.DepartmentId, f => f.PickRandom(departments).Id)
            .RuleFor(e => e.DesignationId, f => f.PickRandom(designations).Id)
            .RuleFor(e => e.Salary, f => f.Random.Decimal(30000, 140000))
            .RuleFor(e => e.JoiningDate, f => DateOnly.FromDateTime(f.Date.Past(10)))
            .RuleFor(e => e.Phone, f => f.Phone.PhoneNumber("###-###-####"))
            .RuleFor(e => e.Address, f => f.Address.StreetAddress())
            .RuleFor(e => e.City, f => f.Address.City())
            .RuleFor(e => e.Country, f => f.Address.Country());

        var employees = employeeFaker.Generate(EmployeeCount);
        _dbContext.Employees.AddRange(employees);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var projectFaker = new Faker<Project>("en")
            .RuleFor(p => p.Name, f => f.Company.CatchPhrase())
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.Status, f => f.PickRandom<ProjectStatus>())
            .RuleFor(p => p.StartDate, f => DateOnly.FromDateTime(f.Date.Past(2)))
            .RuleFor(p => p.EndDate, (f, p) =>
            {
                if (p.Status != ProjectStatus.Completed)
                {
                    return null;
                }

                var start = p.StartDate.ToDateTime(TimeOnly.MinValue);
                var end = f.Date.Between(start, DateTime.UtcNow);
                return DateOnly.FromDateTime(end);
            });

        var projects = projectFaker.Generate(ProjectCount);
        _dbContext.Projects.AddRange(projects);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var faker = new Faker("en");
        var projectMembers = new List<ProjectMember>();
        var tasks = new List<WorkTask>();

        foreach (var project in projects)
        {
            var memberCount = faker.Random.Int(5, Math.Min(12, employees.Count));
            var memberIds = faker.PickRandom(employees, memberCount)
                .Select(employee => employee.Id)
                .Distinct()
                .ToList();

            foreach (var memberId in memberIds)
            {
                projectMembers.Add(new ProjectMember
                {
                    ProjectId = project.Id,
                    EmployeeId = memberId
                });
            }

            var taskCount = faker.Random.Int(10, 20);
            var projectStart = project.StartDate.ToDateTime(TimeOnly.MinValue);
            for (var index = 0; index < taskCount; index++)
            {
                var assigned = faker.Random.Bool(0.75f) && memberIds.Count > 0
                    ? faker.PickRandom(memberIds)
                    : (int?)null;

                var dueDate = DateOnly.FromDateTime(faker.Date.Between(projectStart, DateTime.UtcNow.AddMonths(6)));

                tasks.Add(new WorkTask
                {
                    ProjectId = project.Id,
                    AssignedEmployeeId = assigned,
                    Title = faker.Hacker.Verb() + " " + faker.Hacker.Noun(),
                    Description = faker.Lorem.Sentence(),
                    Status = faker.PickRandom<TaskStatus>(),
                    Priority = faker.PickRandom<TaskPriority>(),
                    DueDate = dueDate
                });
            }
        }

        _dbContext.ProjectMembers.AddRange(projectMembers);
        _dbContext.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}
