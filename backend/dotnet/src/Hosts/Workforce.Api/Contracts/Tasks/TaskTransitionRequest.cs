using Workforce.AppCore.Domain.Projects;
using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;

namespace Workforce.Api.Contracts.Tasks;

public sealed class TaskTransitionRequest
{
    public ProjectTaskStatus ToStatus { get; init; }
}
