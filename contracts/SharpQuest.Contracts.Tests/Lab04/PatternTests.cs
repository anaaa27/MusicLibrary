namespace SharpQuest.Contracts.Tests.Lab04;

/// <summary>Lab 04: Classify — a switch expression with an arm per kind, a fallback, and a null arm.</summary>
public class PatternTests : Lab04TestBase
{
    [Fact]
    public void Every_kind_gets_a_label()
    {
        for (var kind = 0; kind < Kinds.Count; kind++)
        {
            var label = Wiring.Classify(Sample(kind, 2));
            Assert.False(string.IsNullOrWhiteSpace(label), $"Classify returned nothing for {Describe(Kinds[kind])}.");
        }
    }

    [Fact]
    public void The_label_depends_on_the_kind_not_the_item()
    {
        Assert.Equal(Wiring.Classify(Sample(0, 1)), Wiring.Classify(Sample(0, 2)));
        Assert.Equal(Wiring.Classify(Sample(1, 1)), Wiring.Classify(Sample(1, 2)));
    }

    [Fact]
    public void Different_kinds_get_different_labels()
    {
        var labels = Enumerable.Range(0, Kinds.Count).Select(k => Wiring.Classify(Sample(k, 4))).ToList();
        var shared = labels.GroupBy(l => l, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);

        Assert.True(shared is null,
            $"Two of your kinds share the label \"{shared?.Key}\". Each type pattern needs its own arm.");
    }

    [Fact]
    public void Something_that_is_not_one_of_your_kinds_gets_the_fallback()
    {
        var foreign = new ForeignItem("foreign-1", "Something else entirely");

        var label = Wiring.Classify(foreign);

        Assert.False(string.IsNullOrWhiteSpace(label),
            "Classify returned nothing for an item that isn't one of your kinds. Add the '_ => …' arm.");

        var kindLabels = Enumerable.Range(0, Kinds.Count).Select(k => Wiring.Classify(Sample(k, 3)));
        Assert.DoesNotContain(label, kindLabels);
    }

    [Fact]
    public void Null_is_a_programming_error()
    {
        var wiring = Wiring;   // resolve the wiring before the call, so "not started" skips instead of failing

        var ex = Record.Exception(() => wiring.Classify(null!));

        Assert.True(ex is not null,
            "Classify(null) returned instead of throwing. A null item is a bug in the caller: 'null => throw new ArgumentNullException(...)'.");
        Assert.IsAssignableFrom<ArgumentNullException>(ex);
    }

    [Fact]
    public void Classifying_does_not_change_the_item_or_the_store()
    {
        var store = StoreWith((0, 1), (1, 2));
        var before = store.All().Select(i => i.DisplayName).ToList();

        foreach (var item in store.All()) Wiring.Classify(item);

        Assert.Equal(before, store.All().Select(i => i.DisplayName));
    }

    [Fact]
    public void Classify_and_Describe_answer_different_questions()
    {
        var item = Sample(0, 12);

        var label = Wiring.Classify(item);
        var description = Wiring.Describe(item);

        Assert.True(label != description,
            "Classify returns the same text as Describe. Describe is the item speaking for itself (the override); " +
            "Classify is the outside world sorting it into a category (the switch expression).");
    }
}
