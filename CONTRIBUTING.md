# Contributing to Spur

Thank you for your interest in contributing to Spur! This document provides guidelines and instructions for contributing.

## Code of Conduct

Be respectful, inclusive, and professional in all interactions.

## Development Setup

### Prerequisites

- .NET SDK 10.0 or later
- Visual Studio 2026, VS Code, or Rider
- Git

### Getting Started

```bash
git clone https://github.com/AdelSS04/Spur.git
cd Spur
dotnet restore
dotnet build
dotnet test
```

## Coding Standards

### C# Style

- Use C# 13 features (file-scoped namespaces, primary constructors, collection expressions)
- Follow .editorconfig rules
- Use nullable reference types
- Private fields start with underscore: `_fieldName`
- Prefer `sealed` for concrete classes
- Use `readonly struct` for value types

### Async/Await

- Always include `CancellationToken` parameters
- Use `ConfigureAwait(false)` in library code
- Use `Task<Result<T>>` for async operations

### Documentation

- All public APIs must have XML documentation
- Include code examples in XML docs for complex APIs
- Keep comments concise and focused on "why", not "what"

## Pull Request Process

1. **Fork** the repository
2. **Create a feature branch**: `git checkout -b feature/my-feature`
3. **Make your changes** with clear, atomic commits
4. **Add tests** for new functionality
5. **Ensure all tests pass**: `dotnet test`
6. **Update documentation** if needed
7. **Submit a pull request** with a clear description

### PR Requirements

- ✅ All tests pass
- ✅ Code coverage maintained or improved
- ✅ No compiler warnings
- ✅ XML documentation for public APIs
- ✅ CHANGELOG.md updated

## Testing

- Write unit tests for all new features
- Use xUnit for test framework
- Use Moq for mocking dependencies
- Aim for 90%+ coverage in core package
- Include both success and failure scenarios

## Project Structure

```
src/                  # Source packages
tests/                # Test projects
benchmarks/           # Performance benchmarks
samples/              # Sample applications
docs/                 # Documentation
```

## Reporting Issues

- Use GitHub Issues
- Provide a clear description and reproduction steps
- Include .NET version and OS information
- Add code samples if applicable

## Security

Please report security vulnerabilities to security@spur.adellajil.com. Do not open public issues for security concerns.

## Questions?

Open a discussion on GitHub or reach out to the maintainers.
