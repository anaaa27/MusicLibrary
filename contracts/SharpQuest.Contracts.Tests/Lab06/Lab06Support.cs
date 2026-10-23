using System.Reflection;
using SharpQuest.Contracts.Tests.Support;

namespace SharpQuest.Contracts.Tests.Lab06;

/// <summary>Shared by every Lab 06 test class.</summary>
public abstract class Lab06TestBase
{
    private ILab06Wiring? _wiringCache;

    protected ILab06Wiring Wiring => _wiringCache ??= StudentCode.Wiring<ILab06Wiring>();

    protected Type PublisherType =>
        Wiring.PublisherType ?? Fail<Type>("PublisherType is null. Return the type that declares your event.");

    protected IItemStore StoreWith(params int[] seeds)
    {
        var store = Wiring.CreateStore();
        Assert.True(store is not null, "CreateStore() returned null.");
        foreach (var seed in seeds) store!.Add(Wiring.CreateSample(seed));
        return store!;
    }

    /// <summary>The public events the publisher declares. `event` is a keyword; a plain field isn't one.</summary>
    protected static IReadOnlyList<EventInfo> EventsOf(Type type) =>
        type.GetEvents(BindingFlags.Public | BindingFlags.Instance);

    /// <summary>Public events declared `static` — one subscriber list shared by every instance.</summary>
    protected static IReadOnlyList<EventInfo> StaticEventsOf(Type type) =>
        type.GetEvents(BindingFlags.Public | BindingFlags.Static);

    /// <summary>Records what a subscriber saw, so a test can read it back.</summary>
    protected sealed class Recorder
    {
        public List<IItem> Seen { get; } = [];

        public int Count => Seen.Count;

        public Action<IItem> Handler => item => Seen.Add(item);

        public IReadOnlyList<string> Ids => Seen.Select(i => i.Id).ToList();
    }

    protected static string Name(Type t) => t.Name;

    protected static T Fail<T>(string message)
    {
        Assert.Fail(message);
        return default!;   // never reached
    }
}
