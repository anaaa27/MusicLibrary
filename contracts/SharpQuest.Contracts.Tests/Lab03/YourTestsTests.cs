using SharpQuest.Contracts.Tests.Support;

namespace SharpQuest.Contracts.Tests.Lab03;

/// <summary>
/// Lab 03, required feature: "5+ xUnit tests over your own domain logic".
/// These checks read your compiled test project (tests/Capstone.Tests). Nothing in it is executed here.
/// </summary>
public class YourTestsTests
{
    private static TestAssemblyFacts Facts()
    {
        StudentCode.Wiring<ILab03Wiring>();   // no wiring class yet = lab not started = skipped
        var path = MutationResults.FindTestAssembly();
        Assert.True(path is not null, "Can't find tests/Capstone.Tests/bin/**/Capstone.Tests.dll. Run `sq test` so it gets built.");
        return TestAssemblyFacts.Load(path!);
    }

    [Fact]
    public void At_least_five_tests_exercise_your_own_code()
    {
        var own = Facts().Tests.Where(t => t.UsesOwnCode).ToList();
        Assert.True(own.Count >= 5,
            $"Found {own.Count} test(s) that use your own types ({string.Join(", ", own)}). Lab 03 asks for at least 5. " +
            "Tests that only go through a Wiring class, or only test SearchMatcher, don't count here.");
    }

    [Fact]
    public void At_least_one_test_checks_that_your_code_rejects_bad_input()
    {
        var facts = Facts();
        Assert.True(facts.Tests.Any(t => t.UsesOwnCode && t.ChecksException),
            "None of your own-code tests uses Assert.Throws (or Record.Exception). Test at least one rule your types enforce.");
    }

    [Fact]
    public void At_least_one_theory_runs_several_cases()
    {
        var facts = Facts();
        Assert.True(facts.Tests.Any(t => t.IsTheory && (t.DataRows >= 2 || t.HasDataSource)),
            "No [Theory] with two or more [InlineData] rows (or [MemberData]). Use one where the same check applies to several inputs.");
    }

    [Fact]
    public void There_are_search_matcher_spec_tests()
    {
        var spec = Facts().Tests.Where(t => t.Class.Contains("SearchMatcher", StringComparison.OrdinalIgnoreCase) && t.UsesSearchMatcher).ToList();
        Assert.True(spec.Count >= 4,
            $"Found {spec.Count} SearchMatcher test method(s). Write at least 4, in a class whose name contains 'SearchMatcher', " +
            "each creating the matcher with SearchMatcher.Create().");
    }

    [Fact]
    public void Spec_tests_do_not_peek_at_the_grader()
    {
        var peeking = Facts().Peeking.Distinct().ToList();
        Assert.True(peeking.Count == 0,
            "Your SearchMatcher tests must check behaviour, not detect which version is running: " + string.Join("; ", peeking));
    }
}
