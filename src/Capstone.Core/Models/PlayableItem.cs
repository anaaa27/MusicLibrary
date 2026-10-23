using SharpQuest.Contracts;

namespace Capstone.Core.Models;

public abstract class PlayableItem : IItem
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required Credit Credit { get; init; }
    public required Genre Genre { get; init; }
    public required TimeSpan Duration { get; init; }
    public required int Year { get; init; }

    public string DisplayName => $"{Credit} - {Title}";

   
    public abstract string Describe();

    protected string Card() => $"{DisplayName} ({Duration:mm\\:ss})";
}