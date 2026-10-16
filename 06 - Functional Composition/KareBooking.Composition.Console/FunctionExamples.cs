namespace KareBooking.Composition.Console;

public static class FunctionExamples
{
    public static Func<T, V> Compose<T, U, V>(Func<T, U> f, Func<U, V> g) => input => g(f(input));
    public static Func<int, int> Twice(Func<int, int> function) => value => function(function(value));
}
