namespace Capstone.Core.Models;

public class LiveTrack : PlayableItem
{
    public required string Venue { get; init; }

    public override string Describe() =>
        $"{Card()} [Live performance at {Venue}]";
}