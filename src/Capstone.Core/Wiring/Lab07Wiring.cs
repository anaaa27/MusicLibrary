using System;
using System.Collections.Generic;
using System.Linq;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab07Wiring : ILab07Wiring
{
    public Type ExtensionHost => typeof(ItemQueries);

    public IItemStore CreateStore() => new MusicLibraryStore();

    public IItem CreateSample(int seed) => new StudioTrack
    {
        Id = $"trk-{seed:D3}",
        Title = $"Sample Track {seed}",
        Credit = new Credit($"Artist {seed % 5}"),
        Genre = Genre.Rock,
        Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(seed % 60),
        Year = 2020 + (seed % 6),
        AlbumTitle = $"Album {seed % 3}"
    };

    public IEnumerable<string> Names(IItemStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        return ((MusicLibraryStore)store).Select(i => i.DisplayName);
    }

    public IReadOnlyList<IItem> Sorted(IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items.OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(i => i.Id, StringComparer.Ordinal)
                    .ToList();
    }

    public IReadOnlyList<IItem> Page(IEnumerable<IItem> items, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);

        return items.OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Id, StringComparer.Ordinal)
            .Skip(skip)
            .Take(take)
            .ToList();
    }

    public IReadOnlyList<IItem> Longest(IEnumerable<IItem> items, int n)
    {
        return items.Longest(n).ToList();
    }

   
    public StoreStats Summary(IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var list = items.ToList();
        if (list.Count == 0)
        {
            return new StoreStats(0, 0, null, 0);
        }

        int count = list.Count;

        int distinctInitials = list
            .Where(i => !string.IsNullOrEmpty(i.DisplayName))
            .Select(i => char.ToUpperInvariant(i.DisplayName[0]))
            .Distinct()
            .Count();

        string longestName = list
            .OrderByDescending(i => i.DisplayName.Length)
            .ThenBy(i => i.Id, StringComparer.Ordinal)
            .First()
            .DisplayName;

        double averageLength = list.Average(i => i.DisplayName.Length);

        return new StoreStats(count, distinctInitials, longestName, averageLength);
    }
}