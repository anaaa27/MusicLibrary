using System.Collections;

namespace Capstone.Core;

public class RecentlyUsed<T> : IEnumerable<T>
{
    private readonly int _capacity;
    private readonly List<T> _items = new();
    private readonly IEqualityComparer<T> _comparer = EqualityComparer<T>.Default;

    public RecentlyUsed(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be at least 1.");

        _capacity = capacity;
    }

    public int Capacity => _capacity;
    public int Count => _items.Count;

    public void Push(T value)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_comparer.Equals(_items[i], value))
            {
                _items.RemoveAt(i);
                break;
            }
        }
        
        _items.Insert(0, value);

        if (_items.Count > _capacity)
        {
            _items.RemoveAt(_items.Count - 1);
        }
    }

    public IEnumerable<T> Items() => _items.AsReadOnly();

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}