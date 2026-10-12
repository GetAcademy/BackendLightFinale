namespace KareBooking.V0.DomainModel;

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

}
