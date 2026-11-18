# C# coding instructions

- Apply these rules whenever generating, modifying, or refactoring C# code. Readability and clarity are more important than brevity or fewer lines.
- Follow the manually written examples in `SchoolManagement.Tests/Domain/StudentTests.cs`.
- Prefer block-bodied methods; avoid excessive `=>`, ternary expressions, and compact syntax. Use explicit control flow and braces.
- Prefer explicit local variables for meaningful test data and clear Arrange / Act / Assert structure.
- Each unit test should focus on one business behavior. Do not combine unrelated scenarios.
- Name tests `MethodName_WhenSpecificCondition_ShouldExpectedResult`; avoid vague names.
- Preserve all existing business rules and test coverage when refactoring.
- Do not introduce unnecessary abstractions; follow the existing DDD architecture.

- Never prioritize fewer lines over code clarity. Use `ShouldThrow`, not `ShouldThrows`, in test names.
