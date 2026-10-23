// Lab 06 — Functions as values & events
// Two ideas that look unrelated and are the same idea:
//   a function can be a value you pass around, and an event is a list of functions
//   other people gave you, called when something happens.
//
// Nothing from Labs 02–05 may break. Those checks run every time, as usual.

namespace SharpQuest.Contracts;

/// <summary>
/// Tells the Lab 06 tests about your event and your functions.
/// Implement once, in src/Capstone.Core/Wiring/.
///
/// <para>What the tests expect of your publisher (usually your store):</para>
/// <list type="bullet">
/// <item>it declares a public <c>event</c> — the keyword, not a plain <c>Action</c> field;</item>
/// <item>the event is raised <b>after</b> an item has actually been added, once per added item,
///       with that item;</item>
/// <item>an <c>Add</c> that throws (a duplicate Id, a null item) raises nothing;</item>
/// <item>adding with nobody subscribed does not throw — that's what <c>?.Invoke</c> is for.</item>
/// </list>
/// </summary>
public interface ILab06Wiring
{
    IItemStore CreateStore();

    /// <summary>A valid item; same contract as <see cref="ILab02Wiring.CreateSample"/>.</summary>
    IItem CreateSample(int seed);

    /// <summary>The type that declares your event (your store, or a small publisher of your own).</summary>
    Type PublisherType { get; }

    /// <summary>
    /// Subscribes <paramref name="handler"/> to your "an item was added" event, and returns the
    /// <b>unsubscribe</b>: calling it detaches exactly that subscription and nothing else.
    /// <para>
    /// In your code this is two lines: <c>store.ItemAdded += handler;</c> and
    /// <c>return () =&gt; store.ItemAdded -= handler;</c>
    /// </para>
    /// Subscribing the same handler twice means it is called twice; unsubscribing once leaves one.
    /// </summary>
    Action Subscribe(IItemStore store, Action<IItem> handler);

    /// <summary>
    /// The items the <paramref name="keep"/> function says yes to, in store order.
    /// The function is the caller's rule — call it once per item and don't second-guess it.
    /// A null function is a programming error: <see cref="ArgumentNullException"/>.
    /// </summary>
    IReadOnlyList<IItem> Where(IItemStore store, Func<IItem, bool> keep);

    /// <summary>
    /// One string per item, in store order, each produced by <paramref name="describe"/>.
    /// A null function throws <see cref="ArgumentNullException"/>.
    /// </summary>
    IReadOnlyList<string> Map(IItemStore store, Func<IItem, string> describe);

    /// <summary>
    /// Runs <paramref name="action"/> on every item, in store order. Nothing is returned:
    /// the caller's function is the whole point. A null action throws <see cref="ArgumentNullException"/>.
    /// </summary>
    void ForEach(IItemStore store, Action<IItem> action);
}
