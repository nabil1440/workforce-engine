# 1. SQL DOMAIN

## SQL Entities (relational, strict)

### Employee

```text
Employee
- Id (int, PK)
- FirstName (string, required)
- LastName (string, required)
- Email (string, required, unique)
- IsActive (bool)
- DepartmentId (FK)
- DesignationId (FK)
- Salary (decimal)
- JoiningDate (date)
- Phone (string, nullable)
- Address (string, nullable)
- City (string, nullable)
- Country (string, nullable)
```

---

### Department

```text
Department
- Id (int, PK)
- Name (string, unique)
```

---

### Designation

```text
Designation
- Id (int, PK)
- Name (string, unique)
```

---

### Project

```text
Project
- Id (int, PK)
- Name (string)
- Description (string, nullable)
- Status (enum: Active | Completed | OnHold)
- StartDate (date)
- EndDate (date, nullable)
```

---

### ProjectMember (join table)

```text
ProjectMember
- ProjectId (FK)
- EmployeeId (FK)
```

---

### Task

```text
Task
- Id (int, PK)
- ProjectId (FK)
- AssignedEmployeeId (FK, nullable)
- Title (string)
- Description (string, nullable)
- Status (enum: Todo | InProgress | Review | Done)
- Priority (enum or int)
- DueDate (date)
```

---

## SQL API Endpoints (MANDATORY)

### Employees

```http
GET    /api/v1/employees
POST   /api/v1/employees
GET    /api/v1/employees/{id}
PUT    /api/v1/employees/{id}
DELETE /api/v1/employees/{id}   // soft delete
```

Query params (must):

```text
?page
?pageSize
&departmentId
&isActive
&search
&sort
```

Events:

- EmployeeCreated
- EmployeeUpdated
- EmployeeDeactivated

---

### Departments & Designations

```http
GET /api/v1/departments
GET /api/v1/designations
```

Read-only. Seeded.

---

### Projects

```http
GET  /api/v1/projects
POST /api/v1/projects
GET  /api/v1/projects/{id}
```

Events:

- ProjectCreated
- ProjectUpdated
- ProjectStatusChanged

---

### Project Members (2nd order mandatory)

```http
POST   /api/v1/projects/{id}/members
DELETE /api/v1/projects/{id}/members/{employeeId}
```

---

### Tasks

```http
POST /api/v1/projects/{projectId}/tasks
GET  /api/v1/projects/{projectId}/tasks
PUT  /api/v1/tasks/{taskId}
POST /api/v1/tasks/{taskId}/transition
```

Transition payload:

```json
{
  "toStatus": "InProgress"
}
```

Events:

- TaskCreated
- TaskAssigned
- TaskStatusChanged

---

# 2. MONGO DOMAIN

## Mongo Documents (document-oriented)

### LeaveRequest

```text
LeaveRequest
- _id (ObjectId)
- employeeId (int)           // SQL reference
- employeeName (string)      // denormalized
- leaveType (string: Sick | Casual | Annual | Unpaid)
- startDate (date)
- endDate (date)
- status (string: Pending | Approved | Rejected | Cancelled)
- reason (string, nullable)
- approvalHistory: [
    {
      status (string)
      changedBy (string)
      changedAt (date)
      comment (string, nullable)
    }
  ]
- createdAt (date)
```

---

### AuditLog

```text
AuditLog
- _id (ObjectId)
- eventType (string)
- entityType (string)
- entityId (string)
- timestamp (date)
- actor (string)
- before (object)
- after (object)
```

Generated **only by .NET worker**.

---

### DashboardSummary

```text
DashboardSummary
- _id (ObjectId)
- generatedAt (date)
- headcountByDepartment [{ department, count }]
- activeProjectsCount (number)
- tasksByStatus [{ status, count }]
- leaveStats [{ type, status, count }]
```

Generated **only by Node worker**.

---

## Mongo API Endpoints (MANDATORY)

### Leave Requests

```http
GET  /api/v1/leaves
POST /api/v1/leaves
GET  /api/v1/leaves/{id}
```

Filters (2nd order):

```text
?status
&leaveType
```

---

### Leave Approval Flow

```http
POST /api/v1/leaves/{id}/approve
POST /api/v1/leaves/{id}/reject
POST /api/v1/leaves/{id}/cancel
```

Events:

- LeaveRequested
- LeaveApproved
- LeaveRejected
- LeaveCancelled

---

### Audit Logs

```http
GET /api/v1/audit
GET /api/v1/audit/entity/{entityType}/{entityId}
```

Read-only. Immutable.

---

### Dashboard

```http
GET /api/v1/dashboard/summary
```

Reads **precomputed Mongo doc only**.
No live aggregation. Ever.

---

# 3. Non-negotiable rules (put this at the bottom of the context file)

```text
- API never calls workers directly
- Workers never expose business APIs
- All cross-service communication is event-driven
- SQL is source of truth for org + work
- Mongo is source of truth for history + reporting
- Domain events are published only after successful commits
```
