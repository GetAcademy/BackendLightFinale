namespace KareBooking.V5;

public sealed class Result<T>
{
    private readonly T? value;
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public T Value => IsSuccess ? value! : throw new InvalidOperationException(Error);

    private Result(bool isSuccess, T? value, string error)
    {
        IsSuccess = isSuccess;
        this.value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, "");
    public static Result<T> Failure(string error) => new(false, default, error);

    public Result<U> Map<U>(Func<T, U> map) => IsSuccess
        ? Result<U>.Success(map(Value)) : Result<U>.Failure(Error);

    // Bind (often called FlatMap) avoids Result<Result<U>> and skips failed steps.
    public Result<U> Bind<U>(Func<T, Result<U>> bind) => IsSuccess
        ? bind(Value) : Result<U>.Failure(Error);

}
