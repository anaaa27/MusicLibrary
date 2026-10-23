using SharpQuest.Contracts.Tests.Support;

namespace SharpQuest.Contracts.Tests.Lab03;

/// <summary>
/// Lab 03: how good are your SearchMatcher tests? `sq test` ran them against eight broken matchers first.
/// A broken matcher is "caught" when at least one of your tests fails on it.
/// </summary>
public class MutationTests
{
    private static MutationResults Results()
    {
        StudentCode.Wiring<ILab03Wiring>();
        return MutationResults.LoadOrSkip();
    }

    [Fact]
    public void All_your_tests_pass()
    {
        var r = Results();
        Assert.True(r.OwnTotal > 0, "No tests ran in tests/Capstone.Tests.");
        Assert.True(r.OwnFailed == 0, $"{r.OwnFailed} of your {r.OwnTotal} tests fail on the correct code. Fix them first: a red test proves nothing.");
    }

    [Fact]
    public void Spec_tests_pass_on_the_correct_matcher()
    {
        var r = Results();
        Assert.True(r.SpecTotal > 0, "No SearchMatcher tests ran. Put them in a class whose name contains 'SearchMatcher'.");
        Assert.True(r.SpecFailed == 0,
            $"{r.SpecFailed} of your SearchMatcher tests fail on the CORRECT matcher. Re-read the rules: the test is wrong, not the matcher.");
    }

    [Theory]
    [InlineData(1, "R2: it is case-sensitive")]
    [InlineData(2, "R3: it doesn't ignore diacritics")]
    [InlineData(3, "R4: it accepts ANY word instead of ALL words")]
    [InlineData(4, "R1: an empty query matches nothing")]
    [InlineData(5, "R5: word order matters")]
    [InlineData(6, "R6: words only match at the start of a word")]
    [InlineData(7, "R7: a null text throws")]
    [InlineData(8, "R8: a null query doesn't throw")]
    public void Broken_matcher_is_caught(int mutant, string whatIsBroken)
    {
        var r = Results();
        Assert.True(r.SpecTotal > 0 && r.SpecFailed == 0,
            "Broken matchers are only tried once your SearchMatcher tests pass on the correct one.");
        if (MutationResults.FindTestAssembly() is { } path && TestAssemblyFacts.Load(path).Peeking.Count > 0)
            Assert.Fail("Not credited: your SearchMatcher tests try to detect which matcher is running (see Spec_tests_do_not_peek_at_the_grader).");
        Assert.True(r.Caught.GetValueOrDefault(mutant),
            $"Broken matcher #{mutant} got past your tests. What's wrong with it: {whatIsBroken}. Add a test for that rule.");
    }
}
