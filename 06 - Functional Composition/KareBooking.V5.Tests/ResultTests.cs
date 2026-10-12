using KareBooking.V5.DomainModel;

using NUnit.Framework;

namespace KareBooking.V5.Tests;

public sealed class ResultTests
{
    [Test]
    public void Map_transforms_a_success() => Assert.That(Result<int>.Success(2).Map(x => x * 3).Value, Is.EqualTo(6));

    [Test]
    public void Bind_flattens_a_step_that_can_fail()
    {
        var success = Result<int>.Success(2).Bind(x => Result<string>.Success($"{x} hours"));
        Assert.That(success.Value, Is.EqualTo("2 hours"));
        Assert.That(Result<int>.Success(2).Bind(_ => Result<int>.Failure("unavailable")).Error, Is.EqualTo("unavailable"));
    }

    [Test]
    public void Failure_short_circuits_Map_and_Bind_and_preserves_the_first_error()
    {
        var calls = 0;
        var result = Result<int>.Failure("first error")
            .Bind(x => { calls++; return Result<int>.Success(x + 1); })
            .Map(x => { calls++; return x * 2; });
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("first error"));
        Assert.That(calls, Is.Zero);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    [Test]
    public void A_failure_in_the_middle_skips_all_later_steps()
    {
        var calls = 0;
        var result = Result<int>.Success(1)
            .Bind(_ => Result<int>.Failure("slot taken"))
            .Bind(x => { calls++; return Result<int>.Success(x); })
            .Map(x => { calls++; return x.ToString(); });
        Assert.That(result.Error, Is.EqualTo("slot taken"));
        Assert.That(calls, Is.Zero);
    }
}
