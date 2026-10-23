using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public class Lab04Wiring : ILab04Wiring
{
    public Type BaseType => typeof(PlayableItem);

    public IReadOnlyList<Type> KindTypes => new[]
    {
        typeof(StudioTrack),
        typeof(LiveTrack)
    };

    public IItemStore CreateStore() => new MusicLibraryStore();

    public IItem CreateOfKind(int kind, int seed) => kind switch
    {
        0 => new StudioTrack
        {
            Id = $"trk-studio-{seed:D4}",
            Title = $"Studio Track {seed}",
            Credit = new Credit($"Band {seed}"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(seed % 50),
            Year = 2020,
            AlbumTitle = $"Album Vol. {seed}"
        },
        _ => new LiveTrack
        {
            Id = $"trk-live-{seed:D4}",
            Title = $"Live Track {seed}",
            Credit = new Credit($"Artist {seed}"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(seed % 50),
            Year = 2021,
            Venue = $"Wembley Stadium #{seed}"
        }
    };

    public string Describe(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item is PlayableItem playable)
            return playable.Describe();

        return item.DisplayName;
    }

    public IReadOnlyList<string> DescribeAll(IItemStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var descriptions = new List<string>();
        foreach (var item in store.All())
        {
            descriptions.Add(Describe(item));
        }

        return descriptions;
    }

    public string Classify(IItem item) => item switch
    {
        null => throw new ArgumentNullException(nameof(item)),
        PlayableItem { Duration.TotalMinutes: >= 8 } => "an epic",
        PlayableItem { Year: < 1980 } => "a classic",
        StudioTrack => "studio",
        LiveTrack => "live",
        _ => "something else"
    };

    public bool TryFindByName(IItemStore store, string name, out IItem? found)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (string.IsNullOrWhiteSpace(name))
        {
            found = null;
            return false;
        }

        found = store.All().FirstOrDefault(item =>
            string.Equals(item.DisplayName, name, StringComparison.OrdinalIgnoreCase));

        return found is not null;
    }

    public int AddAll(IItemStore store, params IItem[] items)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(items);

        int count = 0;
        foreach (var item in items)
        {
            ArgumentNullException.ThrowIfNull(item);
            store.Add(item);
            count++;
        }

        return count;
    }
}