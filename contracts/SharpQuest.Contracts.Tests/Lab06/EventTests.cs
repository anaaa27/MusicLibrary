namespace SharpQuest.Contracts.Tests.Lab06;

/// <summary>Lab 06: one publisher, several subscribers, and the unsubscribe that actually works.</summary>
public class EventTests : Lab06TestBase
{
    [Fact]
    public void Wiring_class_can_be_created()
    {
        Assert.NotNull(Wiring);
    }

    [Fact]
    public void The_publisher_declares_a_public_event()
    {
        var events = EventsOf(PublisherType);

        Assert.True(events.Count > 0 || StaticEventsOf(PublisherType).Count == 0,
            $"{Name(PublisherType)}'s event is declared 'static', so every store in the program would share " +
            "one subscriber list. Make it an instance member.");

        Assert.True(events.Count > 0,
            $"{Name(PublisherType)} declares no public event. A public Action field is not an event: " +
            "with the 'event' keyword, only the class itself can raise it or clear the list.");
    }

    [Fact]
    public void Adding_an_item_tells_the_subscriber()
    {
        var store = Wiring.CreateStore();
        var listener = new Recorder();
        Wiring.Subscribe(store, listener.Handler);

        var item = Wiring.CreateSample(1);
        store.Add(item);

        Assert.True(listener.Count == 1, $"The subscriber was called {listener.Count} times for one Add, not once.");
        Assert.Same(item, listener.Seen[0]);
    }

    [Fact]
    public void Two_subscribers_both_hear_it_in_the_order_they_subscribed()
    {
        var store = Wiring.CreateStore();
        var order = new List<string>();
        Wiring.Subscribe(store, _ => order.Add("first"));
        Wiring.Subscribe(store, _ => order.Add("second"));

        store.Add(Wiring.CreateSample(1));

        Assert.Equal("first,second", string.Join(",", order));
    }

    [Fact]
    public void The_store_is_already_updated_when_the_subscriber_runs()
    {
        var store = Wiring.CreateStore();
        var counts = new List<int>();
        Wiring.Subscribe(store, _ => counts.Add(store.Count));

        store.Add(Wiring.CreateSample(1));
        store.Add(Wiring.CreateSample(2));

        Assert.Equal("1,2", string.Join(",", counts));
    }

    [Fact]
    public void Unsubscribing_stops_that_one_and_leaves_the_other()
    {
        var store = Wiring.CreateStore();
        var leaving = new Recorder();
        var staying = new Recorder();
        var unsubscribe = Wiring.Subscribe(store, leaving.Handler);
        Wiring.Subscribe(store, staying.Handler);

        store.Add(Wiring.CreateSample(1));
        unsubscribe();
        store.Add(Wiring.CreateSample(2));

        Assert.True(leaving.Count == 1, $"The unsubscribed handler was called {leaving.Count} times, not once. '-=' has to detach it.");
        Assert.True(staying.Count == 2, $"The other handler was called {staying.Count} times, not twice. Unsubscribing one must not detach the rest.");
    }

    [Fact]
    public void Unsubscribing_everyone_is_safe()
    {
        var store = Wiring.CreateStore();
        var listener = new Recorder();
        var unsubscribe = Wiring.Subscribe(store, listener.Handler);
        unsubscribe();

        var ex = Record.Exception(() => store.Add(Wiring.CreateSample(1)));

        Assert.True(ex is null, $"Adding with nobody subscribed threw {ex?.GetType().Name}. Raise it with '?.Invoke(...)'.");
        Assert.Equal(0, listener.Count);
    }

    [Fact]
    public void Nobody_subscribed_is_not_a_crash()
    {
        var store = Wiring.CreateStore();

        var ex = Record.Exception(() => store.Add(Wiring.CreateSample(1)));

        Assert.True(ex is null,
            $"Adding to a store with no subscribers threw {ex?.GetType().Name}. " +
            "An event with nobody attached is null: raise it with '?.Invoke(...)'.");
    }

    [Fact]
    public void The_same_handler_twice_is_called_twice()
    {
        var store = Wiring.CreateStore();
        var listener = new Recorder();
        var first = Wiring.Subscribe(store, listener.Handler);
        Wiring.Subscribe(store, listener.Handler);

        store.Add(Wiring.CreateSample(1));

        Assert.True(listener.Count == 2, $"A handler subscribed twice was called {listener.Count} times, not twice. An event is a list, not a set.");

        first();
        store.Add(Wiring.CreateSample(2));

        Assert.True(listener.Count == 3, $"After unsubscribing one of the two, the handler was called {listener.Count - 2} times for the second Add, not once.");
    }

    [Fact]
    public void A_duplicate_that_is_rejected_tells_nobody()
    {
        var store = Wiring.CreateStore();
        var item = Wiring.CreateSample(1);
        store.Add(item);

        var listener = new Recorder();
        Wiring.Subscribe(store, listener.Handler);
        Record.Exception(() => store.Add(Wiring.CreateSample(1)));

        Assert.True(listener.Count == 0,
            "An Add that threw (a duplicate Id) still raised the event. Raise it after the item is really in.");
    }

    [Fact]
    public void A_null_item_that_is_rejected_tells_nobody()
    {
        var store = Wiring.CreateStore();
        var listener = new Recorder();
        Wiring.Subscribe(store, listener.Handler);

        Record.Exception(() => store.Add(null!));

        Assert.True(listener.Count == 0, "An Add that threw (a null item) still raised the event.");
    }

    [Fact]
    public void Every_added_item_is_announced_once_in_order()
    {
        var store = Wiring.CreateStore();
        var listener = new Recorder();
        Wiring.Subscribe(store, listener.Handler);

        foreach (var seed in new[] { 4, 1, 7 }) store.Add(Wiring.CreateSample(seed));

        Assert.Equal(string.Join(",", store.All().Select(i => i.Id)), string.Join(",", listener.Ids));
    }

    [Fact]
    public void Subscribers_of_one_store_do_not_hear_another_store()
    {
        var mine = Wiring.CreateStore();
        var theirs = Wiring.CreateStore();
        var listener = new Recorder();
        Wiring.Subscribe(mine, listener.Handler);

        theirs.Add(Wiring.CreateSample(1));

        Assert.True(listener.Count == 0,
            "A subscriber of one store heard another store's Add. A 'static' event is shared by every instance — make it an instance member.");
    }
}
