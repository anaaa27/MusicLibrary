using System.Reflection;
using SharpQuest.Contracts.Tests.Support;

namespace SharpQuest.Contracts.Tests.Lab04;

/// <summary>Shared by every Lab 04 test class: the wiring, the hierarchy it points at, and small helpers.</summary>
public abstract class Lab04TestBase
{
    private ILab04Wiring? _wiringCache;

    protected ILab04Wiring Wiring => _wiringCache ??= StudentCode.Wiring<ILab04Wiring>();

    protected Type BaseType => Wiring.BaseType ?? Fail<Type>("BaseType is null. Return your abstract base class.");

    protected IReadOnlyList<Type> Kinds
    {
        get
        {
            var kinds = Wiring.KindTypes;
            if (kinds is null || kinds.Count < 2)
                Fail<object>($"KindTypes has {kinds?.Count ?? 0} type(s). Lab 04 needs at least two concrete kinds deriving from your base.");
            return kinds!;
        }
    }

    protected IItem Sample(int kind, int seed)
    {
        var item = Wiring.CreateOfKind(kind, seed);
        Assert.True(item is not null, $"CreateOfKind({kind}, {seed}) returned null. It must return an item of KindTypes[{kind}].");
        return item!;
    }

    protected IItemStore StoreWith(params (int Kind, int Seed)[] items)
    {
        var store = Wiring.CreateStore();
        Assert.True(store is not null, "CreateStore() returned null.");
        foreach (var (kind, seed) in items) store!.Add(Sample(kind, seed));
        return store!;
    }

    /// <summary>
    /// The members the base declares as abstract or virtual, take no parameters and return a string.
    /// These are the polymorphic ones: at least one must exist, and every kind must override one of them.
    /// </summary>
    protected static IReadOnlyList<MethodInfo> PolymorphicMembers(Type baseType)
    {
        return baseType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType == typeof(string))
            .Where(m => m.GetParameters().Length == 0)
            .Where(m => m.IsVirtual && !m.IsFinal)
            .Where(m => m.Name is not ("ToString" or "get_Id" or "get_DisplayName"))
            .ToList();
    }

    /// <summary>The kind's own override of <paramref name="declared"/>, or null when it doesn't override it.</summary>
    protected static MethodInfo? OverrideIn(Type kind, MethodInfo declared)
    {
        var actual = kind.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == declared.Name
                                 && m.GetParameters().Length == 0
                                 && m.ReturnType == typeof(string)
                                 && m.GetBaseDefinition() == declared.GetBaseDefinition());
        return actual?.DeclaringType == kind ? actual : null;
    }

    protected static string Describe(Type t) => t.Name;

    protected static T Fail<T>(string message)
    {
        Assert.Fail(message);
        return default!;   // never reached
    }
}
