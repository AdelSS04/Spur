namespace Spur.Tests.Helpers;

public static class TestData
{
    public static class Errors
    {
        public static readonly Error NotFound = Error.NotFound("Resource not found", "TEST_NOT_FOUND");
        public static readonly Error Validation = Error.Validation("Validation failed", "TEST_VALIDATION");
        public static readonly Error Unauthorized = Error.Unauthorized("Unauthorized access", "TEST_UNAUTHORIZED");
        public static readonly Error Conflict = Error.Conflict("Resource conflict", "TEST_CONFLICT");
        public static readonly Error Unexpected = Error.Unexpected("Unexpected error", "TEST_UNEXPECTED");
    }

    public static class Values
    {
        public const int IntValue = 42;
        public const string StringValue = "test-value";
        public const bool BoolValue = true;
        public const double DoubleValue = 3.14;
    }

    public static class Messages
    {
        public const string SuccessMessage = "Operation succeeded";
        public const string FailureMessage = "Operation failed";
        public const string ValidationMessage = "Validation error";
    }

    public record TestUser(int Id, string Name, string Email);

    public static readonly TestUser SampleUser = new(1, "Test User", "test@example.com");
    public static readonly TestUser AnotherUser = new(2, "Another User", "another@example.com");
}
