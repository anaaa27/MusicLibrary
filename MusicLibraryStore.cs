using SharpQuest.Contracts;

namespace Capstone.Core;


public class MusicLibraryStore : IItemStore
{
    private readonly List<IItem> _items = [];

    public int Count => _items.Count;

    public void Add(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (_items.Any(i => string.Equals(i.Id, item.Id, StringComparison.Ordinal)))
            throw new ArgumentException($"An item with Id '{item.Id}' already exists.", nameof(item));

        _items.Add(item);
    }

    public IItem? FindById(string id)
    {
        if (id is null) return null;

        return _items.Find(i => string.Equals(i.Id, id, StringComparison.Ordinal));
    }

    public IReadOnlyList<IItem> All()
    {
        return _items.ToList();
    }

    public bool Remove(string id)
    {
        if (id is null) return false;

        var index = _items.FindIndex(i => string.Equals(i.Id, id, StringComparison.Ordinal));
        if (index < 0) return false;

        _items.RemoveAt(index);
        return true;
    }
}