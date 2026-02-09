namespace Workforce.AppCore.Domain.Projects;

public enum ProjectStatus
{
    Active = 0,
    Completed = 1,
    OnHold = 2
}

public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    Review = 2,
    Done = 3
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
