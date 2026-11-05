using System;
using System.Collections.Generic;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public class Lab06Wiring : ILab06Wiring
{
    public Type PublisherType => typeof(MusicLibraryStore);

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

    // 1. Where
    public IReadOnlyList<IItem> Where(IItemStore store, Func<IItem, bool> keep)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(keep);

        var result = new List<IItem>();
        foreach (var item in store.All())
        {
            if (keep(item))
            {
                result.Add(item);
            }
        }
        return result;
    }

    // 2. Map
    public IReadOnlyList<string> Map(IItemStore store, Func<IItem, string> transform)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(transform);

        var result = new List<string>();
        foreach (var item in store.All())
        {
            result.Add(transform(item));
        }
        return result;
    }

    // 3. ForEach
    public void ForEach(IItemStore store, Action<IItem> action)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in store.All())
        {
            action(item);
        }
    }

    // 4. Subscribe
    public Action Subscribe(IItemStore store, Action<IItem> handler)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(handler);

        if (store is MusicLibraryStore concreteStore)
        {
            concreteStore.ItemAdded += handler;
            return () => concreteStore.ItemAdded -= handler;
        }

        throw new InvalidOperationException($"Expected store to be {nameof(MusicLibraryStore)}.");
    }
}