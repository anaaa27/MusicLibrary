using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using SharpQuest.Contracts.Tests.Support;

namespace SharpQuest.Contracts.Tests.Lab05;

/// <summary>Shared by every Lab 05 test class: the wiring, the collection type, and small helpers.</summary>
public abstract class Lab05TestBase
{
    private ILab05Wiring? _wiringCache;

    protected ILab05Wiring Wiring => _wiringCache ??= StudentCode.Wiring<ILab05Wiring>();

    protected Type CollectionType =>
        Wiring.CollectionType ?? Fail<Type>("CollectionType is null. Return the class CreateStore() gives back.");

    protected IItemStore StoreWith(params int[] seeds)
    {
        var store = Wiring.CreateStore();
        Assert.True(store is not null, "CreateStore() returned null.");
        foreach (var seed in seeds) store!.Add(Wiring.CreateSample(seed));
        return store!;
    }

    /// <summary>Indexers, whatever the student called them ("Item" by default, or renamed with IndexerName).</summary>
    protected static PropertyInfo? Indexer(Type type, Type parameterType) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.GetIndexParameters() is [{ } only] && only.ParameterType == parameterType);

    /// <summary>
    /// True when the type contains a method written with <c>yield return</c>: the compiler turns one into a
    /// nested state-machine class implementing IEnumerator. Handing back another collection's enumerator
    /// leaves no such type behind.
    /// </summary>
    protected static bool HasIteratorMember(Type type) =>
        type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .Any(t => t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
                      && typeof(IEnumerator).IsAssignableFrom(t));

    protected static string Name(Type t) => t.Name;

    protected static T Fail<T>(string message)
    {
        Assert.Fail(message);
        return default!;   // never reached
    }
}
