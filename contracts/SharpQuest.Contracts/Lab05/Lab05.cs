// Lab 05 — Collections & generics
// Your store stops being "a list with rules" and becomes a collection:
// foreach works on it, [0] and ["id"] work on it, and it has an iterator you wrote.
// Then one generic type of your own, so `T` stops being something other people write.
//
// Nothing from Labs 02–04 may break. Those checks run every time, as usual.

namespace SharpQuest.Contracts;

/// <summary>
/// Tells the Lab 05 tests about your collection and your generic type.
/// Implement once, in src/Capstone.Core/Wiring/.
///
/// <para>What the tests expect of your collection (the class <see cref="CreateStore"/> returns):</para>
/// <list type="bullet">
/// <item>it still implements <see cref="IItemStore"/>, exactly as in Lab 02;</item>
/// <item>it implements <c>IEnumerable&lt;IItem&gt;</c>, so <c>foreach</c> works on the store itself;</item>
/// <item>at least one member is written with <c>yield return</c> — an iterator you wrote, not the
///       list's enumerator handed back;</item>
/// <item>it has an indexer by position, <c>this[int]</c>, and one by id, <c>this[string]</c>.</item>
/// </list>
///
/// <para>And of your generic type: one type of your own with a type parameter, doing a real job.
/// <see cref="Recent{T}"/> below is that job — see its rules.</para>
/// </summary>
public interface ILab05Wiring
{
    /// <summary>Your store, same contract as Lab 02 — and now a collection as well.</summary>
    IItemStore CreateStore();

    /// <summary>A valid item; same contract as <see cref="ILab02Wiring.CreateSample"/>.</summary>
    IItem CreateSample(int seed);

    /// <summary>The class <see cref="CreateStore"/> returns.</summary>
    Type CollectionType { get; }

    /// <summary>
    /// Your own generic type, as an open definition: <c>typeof(RecentlyUsed&lt;&gt;)</c>.
    /// It has to be a type you wrote, with at least one type parameter, and it has to be what
    /// <see cref="Recent{T}"/> actually uses.
    /// </summary>
    Type GenericDefinition { get; }

    /// <summary>The item at <paramref name="index"/>, through your <c>this[int]</c> indexer,
    /// in the order items were added. Out of range throws <see cref="ArgumentOutOfRangeException"/>.</summary>
    IItem At(IItemStore store, int index);

    /// <summary>The item with this Id, through your <c>this[string]</c> indexer, or null if there is none.</summary>
    IItem? ById(IItemStore store, string id);

    /// <summary>
    /// Every item, collected by <c>foreach</c>-ing over the store itself (not over <c>All()</c>),
    /// in the order items were added.
    /// </summary>
    IReadOnlyList<IItem> EachInOrder(IItemStore store);

    /// <summary>
    /// The items whose <see cref="IItem.DisplayName"/> contains <paramref name="text"/>
    /// (case-insensitive), in store order — written as an iterator, with <c>yield return</c>,
    /// so nothing is built until somebody enumerates it.
    /// </summary>
    IEnumerable<IItem> Matching(IItemStore store, string text);

    /// <summary>
    /// Push each value of <paramref name="pushed"/>, in order, into an instance of your generic type
    /// with this <paramref name="capacity"/>, then return what it holds.
    /// <list type="number">
    /// <item><b>Newest first.</b> The last value pushed comes first.</item>
    /// <item><b>Bounded.</b> It never holds more than <paramref name="capacity"/> values; the oldest drops out.</item>
    /// <item><b>No duplicates.</b> Pushing a value that is already in there moves it to the front instead
    ///       of storing it twice (use <c>EqualityComparer&lt;T&gt;.Default</c>).</item>
    /// <item>A capacity below 1 is a programming error: <see cref="ArgumentOutOfRangeException"/>.</item>
    /// </list>
    /// The tests call this with several different <c>T</c>s, so <c>object</c> in a trench coat won't do.
    /// </summary>
    IReadOnlyList<T> Recent<T>(int capacity, params T[] pushed);
}
