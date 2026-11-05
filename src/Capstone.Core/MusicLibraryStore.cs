using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core;

public class MusicLibraryStore : IItemStore, IEnumerable<IItem>
{
    private readonly List<IItem> _items = new();
    
    public event Action<IItem>? ItemAdded;

    public int Count => _items.Count;

    public void Add(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrWhiteSpace(item.Id))
            throw new ArgumentException("Item ID cannot be null or whitespace.", nameof(item));
        
        if (FindById(item.Id) is not null)
            throw new ArgumentException($"An item with Id '{item.Id}' already exists.", nameof(item));

        _items.Add(item);

        ItemAdded?.Invoke(item);
    }

    public IItem? FindById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return _items.Find(i => string.Equals(i.Id, id, StringComparison.Ordinal));
    }

    public IReadOnlyList<IItem> All() => _items.ToList();

    public bool Remove(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        var item = FindById(id);
        if (item is null)
            return false;

        return _items.Remove(item);
    }

    public IItem this[int index] => _items[index];

    public IItem? this[string id] => string.IsNullOrWhiteSpace(id) ? null : FindById(id);

    public IEnumerator<IItem> GetEnumerator()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<IItem> Matching(string query)
    {
        query ??= string.Empty;

        foreach (var item in _items)
        {
            if (item.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                yield return item;
            }
        }
    }
    
}