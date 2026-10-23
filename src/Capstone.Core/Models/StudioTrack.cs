namespace Capstone.Core.Models;

public class StudioTrack : PlayableItem
{
    public required string AlbumTitle { get; init; }

    public override string Describe() =>
        $"{Card()} [Studio cut from \"{AlbumTitle}\"]";
}