using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public class Lab02Wiring : ILab02Wiring
{
    public IItemStore CreateStore() => new MusicLibraryStore();

    public IItemMapper CreateMapper() => new MusicLibraryMap();

    public IItem CreateSample(int seed)
    {
        return new Track
        {
            Id = $"trk-{seed:D3}",
            Title = $"Song {seed}",
            Credit = new Credit($"Artist {seed}"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(seed % 60),
            Year = 2000 + (seed % 24),
            Rating = (seed % 5) + 1
        };
    }
}