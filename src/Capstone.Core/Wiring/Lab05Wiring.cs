using System;
using System.Collections.Generic;
using System.Linq;
using Capstone.Core;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public class Lab05Wiring : ILab05Wiring
{
    public Type CollectionType => typeof(MusicLibraryStore);
    public Type GenericDefinition => typeof(RecentlyUsed<>);

    public IItemStore CreateStore() => new MusicLibraryStore();
    
    public IItem CreateSample(int seed) => new StudioTrack
    {
        Id = $"trk-{seed:D4}",
        Title = $"Track {seed}",
        Credit = new Credit($"Artist {seed}"),
        Genre = Genre.Rock,
        Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(seed % 60),
        Year = 2024,
        AlbumTitle = $"Album {seed}"
    };

    //index
    public IItem At(IItemStore store, int index)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (store is MusicLibraryStore musicStore)
            return musicStore[index];

        return store.All()[index];
    }

    //library[id]
    public IItem? ById(IItemStore store, string id)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (store is MusicLibraryStore musicStore)
            return musicStore[id];

        return store.All().FirstOrDefault(i => string.Equals(i.Id, id, StringComparison.Ordinal));
    }
    
    public IReadOnlyList<IItem> EachInOrder(IItemStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var list = new List<IItem>();

        if (store is IEnumerable<IItem> enumerable)
        {
            foreach (var item in enumerable)
            {
                list.Add(item);
            }
        }
        else
        {
            foreach (var item in store.All())
            {
                list.Add(item);
            }
        }

        return list;
    }
    
    public IReadOnlyList<T> Recent<T>(int capacity, params T[] items)
    {
        var buffer = new RecentlyUsed<T>(capacity);

        if (items is not null)
        {
            foreach (var item in items)
            {
                buffer.Push(item);
            }
        }

        return buffer.Items().ToList();
    }
    
    public IEnumerable<IItem> Matching(IItemStore store, string query)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (store is MusicLibraryStore musicStore)
            return musicStore.Matching(query);

        query ??= string.Empty;
        return store.All().Where(i => i.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase));
    }
}