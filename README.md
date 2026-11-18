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
- `SchoolManagement.Infrastructure`: Repository lưu trữ trong bộ nhớ.
- `SchoolManagement.Tests`: Unit Tests sử dụng xUnit và Moq.

## Chạy kiểm thử

Yêu cầu **.NET 10 SDK**.

```bash
dotnet test
```
