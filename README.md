# School Management — Unit Testing

Project thực hành Unit Testing bằng **C# / .NET 10**, sử dụng **xUnit** và **Moq**, được tổ chức theo hướng Domain-Driven Design (DDD).

## Nội dung

- Quản lý thông tin và trạng thái sinh viên.
- Kiểm tra dữ liệu đầu vào bằng Value Objects.
- Kiểm tra điều kiện tốt nghiệp.
- Unit Test cho Domain, Application Service và In-Memory Repository.

## Cấu trúc project

- `SchoolManagement.Domain`: Entity, Value Objects và nghiệp vụ.
- `SchoolManagement.Application`: Xử lý các chức năng ứng dụng.
- `SchoolManagement.Infrastructure`: PostgreSQL persistence, migrations, and an in-memory repository for tests.
- `SchoolManagement.API`: ASP.NET Core Web API.
- `SchoolManagement.Tests`: Unit Tests sử dụng xUnit và Moq.

## Chạy kiểm thử

Yêu cầu **.NET 10 SDK**.

```bash
dotnet test
```

## Web API

Install PostgreSQL, create a development database and a database user with permission to create tables, and set `ConnectionStrings__SchoolDatabase` in your environment to the connection string. Supply credentials locally; do not commit them. For example, in PowerShell use `$env:ConnectionStrings__SchoolDatabase = '<your local connection string>'`.

Install the EF Core command-line tool matching EF Core 10, then apply the included migration:

```bash
dotnet tool install --global dotnet-ef --version 10.0.14
dotnet restore
dotnet ef database update --project SchoolManagement.Infrastructure --startup-project SchoolManagement.Infrastructure
```

The EF tools use the same environment variable. The application does not apply migrations automatically. Run the API with the .NET 10 SDK:

```bash
dotnet restore
dotnet run --project SchoolManagement.API --urls http://localhost:5000
```

The OpenAPI document is available at `http://localhost:5000/openapi/v1.json` and can be opened with an OpenAPI client. See `SchoolManagement.API/SchoolManagement.API.http` for sample requests.

| Method | Path | Purpose | JSON body fields |
| --- | --- | --- | --- |
| POST | `/api/students` | Create a student | `studentCode`, `fullName`, `email`, `gradePointAverage`, `earnedCredits`, `hasActiveDisciplinaryAction` |
| GET | `/api/students/{id}` | Get a student | None |
| PUT | `/api/students/{id}/name` | Update name | `fullName` |
| PUT | `/api/students/{id}/email` | Update email | `email` |
| PUT | `/api/students/{id}/academic-record` | Update academic record | `gradePointAverage`, `earnedCredits`, `hasActiveDisciplinaryAction` |
| POST | `/api/students/{id}/activate` | Activate | None |
| POST | `/api/students/{id}/deactivate` | Deactivate | None |
| GET | `/api/students/{id}/graduation-eligibility` | Evaluate graduation | None |
| POST | `/api/students/{id}/graduate` | Graduate | None |

Example creation body:

```json
{
  "studentCode": "STU123456",
  "fullName": "Nguyen An",
  "email": "an@example.com",
  "gradePointAverage": 5,
  "earnedCredits": 120,
  "hasActiveDisciplinaryAction": false
}
```

Creation returns `201` and a `Location` header; other successful operations return `200`. Invalid input returns `400`, a missing student returns `404`, and a duplicate code or invalid state transition returns `409`. Errors use JSON Problem Details with `status` and `title`, plus `detail` or `errors` where applicable. Student `status` is numeric: 1 (active), 2 (inactive), or 3 (graduated).

## PostgreSQL integration tests

Create a separate database whose name ends in `_test` and set `SCHOOL_TEST_DATABASE` to its connection string. The test user needs permission to create and drop schemas and create tables within them. Each test creates a uniquely named schema, applies migrations there, and drops that schema afterward. Never point this variable at a development or production database. If `ConnectionStrings__SchoolDatabase` is set, tests also reject a matching host, port, and database.

```bash
dotnet test --filter "Category!=PostgreSQL"
dotnet test --filter "Category=PostgreSQL"
```

Without `SCHOOL_TEST_DATABASE`, PostgreSQL integration tests are reported as skipped. Other API tests use an in-memory repository and controller tests use a mocked repository.
