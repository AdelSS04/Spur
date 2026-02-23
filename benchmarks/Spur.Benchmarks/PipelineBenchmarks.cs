using BenchmarkDotNet.Attributes;
using Spur.Pipeline;

namespace Spur.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class PipelineBenchmarks
{
    private readonly Result<int> _successResult = Result.Success(10);
    private readonly Result<int> _failureResult = Result.Failure<int>(Error.NotFound("Not found"));

    [Benchmark(Baseline = true)]
    public Result<int> DirectOperation()
    {
        return Result.Success(20);
    }

    [Benchmark]
    public Result<int> SingleThenOnSuccess()
    {
        return _successResult.Then(x => Result.Success(x * 2));
    }

    [Benchmark]
    public Result<int> SingleThenOnFailure()
    {
        return _failureResult.Then(x => Result.Success(x * 2));
    }

    [Benchmark]
    public Result<int> SingleMapOnSuccess()
    {
        return _successResult.Map(x => x * 2);
    }

    [Benchmark]
    public Result<int> SingleMapOnFailure()
    {
        return _failureResult.Map(x => x * 2);
    }

    [Benchmark]
    public Result<string> ChainedOperationsOnSuccess()
    {
        return _successResult
            .Then(x => Result.Success(x * 2))
            .Map(x => x + 5)
            .Validate(x => x > 0, Error.Validation("Must be positive"))
            .Map(x => x.ToString());
    }

    [Benchmark]
    public Result<string> ChainedOperationsOnFailure()
    {
        return _failureResult
            .Then(x => Result.Success(x * 2))
            .Map(x => x + 5)
            .Validate(x => x > 0, Error.Validation("Must be positive"))
            .Map(x => x.ToString());
    }

    [Benchmark]
    public Result<int> TapOnSuccess()
    {
        var sideEffect = 0;
        return _successResult.Tap(x => sideEffect = x);
    }

    [Benchmark]
    public Result<int> RecoverOnFailure()
    {
        return _failureResult.Recover(error => Result.Success(99));
    }
}
