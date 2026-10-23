using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab03Wiring : ILab03Wiring
{
    public IItemStore CreateStore() => new MusicLibraryStore();

    public IItem CreateSample(int seed) =>
        new Track
        {
            Id = $"trk-{seed:D4}",
            Title = $"Song {seed}",
            Credit = new Credit($"Artist {seed}"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(seed % 60),
            Year = 2020 + (seed % 5),
            Rating = 5
        };

    public IReadOnlyList<IItem> Search(IItemStore store, string query)
    {
        var matcher = SearchMatcher.Create();
        return store.All()
            .Where(item => matcher.Matches(item.DisplayName, query))
            .ToList();
    }
}