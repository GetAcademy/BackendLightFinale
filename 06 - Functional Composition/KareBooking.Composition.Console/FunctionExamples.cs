namespace KareBooking.Composition;

public static class FunctionExamples
{
    public static Func<T, V> Compose<T, U, V>(Func<T, U> f, Func<U, V> g) => input => g(f(input));
    public static Func<int, int> Twice(Func<int, int> function) => value => function(function(value));
    public static Func<int, Func<int, int>> AddCurried => first => second => first + second;
    public static Func<int, decimal> PriceWithFirstHour(decimal firstHourPrice) => hours => firstHourPrice + (hours - 1) * 300m;
}
