# School Management

Ứng dụng quản lý sinh viên bằng C# / .NET 10, cung cấp API để quản lý thông tin, trạng thái và xét điều kiện tốt nghiệp.

Project được tổ chức theo Domain-Driven Design (DDD):

- `SchoolManagement.Domain`: thực thể, value object và quy tắc nghiệp vụ.
- `SchoolManagement.Application`: các chức năng ứng dụng.
- `SchoolManagement.Infrastructure`: lưu trữ PostgreSQL, migration và repository in-memory.
- `SchoolManagement.API`: ASP.NET Core Web API.
- `SchoolManagement.Tests`: kiểm thử với xUnit và Moq.

## Cài đặt và chạy

Yêu cầu .NET 10 SDK và PostgreSQL. Tạo database bằng công cụ PostgreSQL, chẳng hạn:

```sql
CREATE DATABASE school_management;
```

Cấu hình `SchoolManagement.API/appsettings.Development.json` trên máy cá nhân:

```json
{
  "ConnectionStrings": {
    "SchoolDatabase": "Host=localhost;Port=5432;Database=school_management;Username=postgres;Password=<mat-khau-cua-ban>"
  }
}
```

Thay thông tin đăng nhập bằng thông tin PostgreSQL của bạn. Không đưa mật khẩu thật vào Git. Tài khoản database cần quyền tạo bảng.

Tại thư mục gốc project, cài EF Core CLI nếu chưa có, sau đó áp dụng migration:

```bash
dotnet tool install --global dotnet-ef --version 10.0.14
dotnet restore
dotnet ef database update --project SchoolManagement.Infrastructure --startup-project SchoolManagement.Infrastructure
```

Chạy API trong môi trường Development (PowerShell):

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project SchoolManagement.API --no-launch-profile --urls http://localhost:5000
```

OpenAPI: `http://localhost:5000/openapi/v1.json`. Các request mẫu nằm trong `SchoolManagement.API/SchoolManagement.API.http`.

## API endpoints

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

Ví dụ tạo sinh viên:

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

Mã sinh viên gồm `STU` và 6 chữ số, phải là duy nhất. Trạng thái trả về là `1` (đang hoạt động), `2` (ngừng hoạt động) hoặc `3` (đã tốt nghiệp).

## Kiểm thử

Chạy toàn bộ kiểm thử bằng `dotnet test`. Unit Test kiểm tra nghiệp vụ và các thành phần với dữ liệu in-memory hoặc mock, không cần PostgreSQL. Để chỉ chạy các bài không dùng PostgreSQL:

```bash
dotnet test --filter "Category!=PostgreSQL"
```

PostgreSQL Integration Test kiểm tra repository với database thật. Tạo database riêng có tên kết thúc bằng `_test`, ví dụ:

```sql
CREATE DATABASE school_management_test;
```

Đặt biến môi trường `SCHOOL_TEST_DATABASE` trỏ tới database test, rồi chạy (ví dụ PowerShell):

```powershell
$env:SCHOOL_TEST_DATABASE = 'Host=localhost;Port=5432;Database=school_management_test;Username=postgres;Password=<mat-khau-cua-ban>'
dotnet test --filter "Category=PostgreSQL"
```

Không trỏ biến này tới database development hoặc production. Tài khoản test cần quyền tạo và xóa schema, tạo bảng; mỗi test tạo schema riêng, áp dụng migration và xóa schema sau khi chạy. Nếu chưa đặt biến, các PostgreSQL Integration Test sẽ được bỏ qua.
