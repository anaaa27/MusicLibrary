using SharpQuest.Contracts;

namespace Capstone.Core.Models;

public sealed class Track : IItem, IPlayable
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required Credit Credit { get; init; }
    public required Genre Genre { get; init; }
    public required TimeSpan Duration { get; init; }
    public int Year { get; init; }
    public int? Rating { get; init; }
    public int PlayCount { get; private set; }

    public string DisplayName => $"{Credit.Artist} - {Title}";

    public void RecordPlay() => PlayCount++;
}