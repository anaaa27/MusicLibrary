// Lab 04 — OOP & pattern matching
// One family of types, one loop that doesn't care which member it holds,
// one switch expression that does, and the two parameter keywords worth knowing.
//
// IMPORTANT: this lab ADDS to your model. Nothing in Lab 02 or Lab 03 may break.
// Your Lab 02 and Lab 03 checks are run again every time, and they must stay green.

namespace SharpQuest.Contracts;

/// <summary>
/// An item the Lab 04 tests hand you that is NOT part of your hierarchy.
/// <see cref="ILab04Wiring.Classify"/> has to cope with it instead of crashing.
/// </summary>
public sealed class ForeignItem : IItem
{
    public ForeignItem(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public string Id { get; }

    public string DisplayName { get; }
}

/// <summary>
/// Tells the Lab 04 tests about the family of types you added. Implement once, in src/Capstone.Core/Wiring/.
///
/// <para>The shape the tests expect:</para>
/// <list type="bullet">
/// <item>
///   <b>A base type</b> (<see cref="BaseType"/>): an <c>abstract class</c> that implements <see cref="IItem"/>
///   and declares at least one <c>abstract</c> or <c>virtual</c> member that takes no parameters and returns a
///   <c>string</c> — a method or a property, your choice. That member is the polymorphic one.
/// </item>
/// <item>
///   <b>Two or more kinds</b> (<see cref="KindTypes"/>): concrete classes deriving from the base, each one
///   <c>override</c>-ing that member with something of its own.
/// </item>
/// </list>
///
/// <para>
/// If your Lab 02 model already has a class that all of this can grow from, grow it: make it abstract and
/// derive from it. If it doesn't, add a new small family beside it. Either way the Lab 02 and Lab 03 tests
/// keep running, so a refactor that breaks your mapper or your store shows up immediately.
/// </para>
/// </summary>
public interface ILab04Wiring
{
    IItemStore CreateStore();

    /// <summary>Your abstract base class.</summary>
    Type BaseType { get; }

    /// <summary>
    /// Your concrete kinds, two or more, each deriving from <see cref="BaseType"/>.
    /// The order is yours; <see cref="CreateOfKind"/> uses the same order.
    /// </summary>
    IReadOnlyList<Type> KindTypes { get; }

    /// <summary>
    /// A valid item of <c>KindTypes[kindIndex]</c>.
    /// The same kind and seed always give the same Id; different kinds or different seeds give different Ids.
    /// Kinds 0 and 1 and seeds 1 to 100 are used.
    /// </summary>
    IItem CreateOfKind(int kindIndex, int seed);

    /// <summary>
    /// The value of the polymorphic member for this item — call it through the base type and let the
    /// override decide. Never empty. Two kinds built from the same seed must describe themselves differently.
    /// </summary>
    string Describe(IItem item);

    /// <summary>
    /// One description per item in the store, in store order, each equal to <see cref="Describe"/> of that item.
    /// Write it as a single loop over the base type: the loop must not know which kind it is holding.
    /// An empty store gives an empty list. Describing never changes the store.
    /// </summary>
    IReadOnlyList<string> DescribeAll(IItemStore store);

    /// <summary>
    /// A short label saying what this item is — written as a <b>switch expression</b> over type patterns.
    /// <list type="bullet">
    /// <item>Items of the same kind get the same label; different kinds get different labels.</item>
    /// <item>An item that is not one of your kinds (such as <see cref="ForeignItem"/>) gets a fallback
    ///       label, different from every kind label. That's the <c>_ =&gt;</c> arm.</item>
    /// <item><c>null</c> is a programming error: <see cref="ArgumentNullException"/>. That's the
    ///       <c>null =&gt; throw</c> arm.</item>
    /// </list>
    /// </summary>
    string Classify(IItem item);

    /// <summary>
    /// Looks for an item whose DisplayName is exactly <paramref name="name"/> (case-sensitive).
    /// True and the item when it's there; false and <c>null</c> when it isn't.
    /// A null or blank name is simply not found — a Try method never throws for that.
    /// Never changes the store.
    /// </summary>
    bool TryFindByName(IItemStore store, string name, out IItem? found);

    /// <summary>
    /// Adds every item, in order, and returns how many were added. Declared with <c>params</c>,
    /// so <c>AddAll(store)</c>, <c>AddAll(store, a)</c> and <c>AddAll(store, a, b, c)</c> all compile.
    /// The store's own rules still apply: a null item or a duplicate Id throws, and whatever was added
    /// before the bad one stays added.
    /// </summary>
    int AddAll(IItemStore store, params IItem[] items);
}
