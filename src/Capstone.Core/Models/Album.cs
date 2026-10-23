namespace Capstone.Core.Models;

public class Album
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Artist { get; init; }
    public int ReleaseYear { get; init; }
    public List<Track> Tracks { get; } = new();

    public void AddTrack(Track track) => Tracks.Add(track);

    public TimeSpan TotalDuration => Tracks.Aggregate(TimeSpan.Zero, (sum, t) => sum + t.Duration);
}