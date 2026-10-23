using SharpQuest.Contracts;

namespace Capstone.Core.Models;

public class Track : PlayableItem
{
    public int PlayCount { get; private set; }
    public int Rating { get; set; } = 5;

    public bool HasReachedCenturionMilestone => PlayCount >= 100;

    public void RecordPlay() => RecordPlays(1);

    public void RecordPlays(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Play count increment cannot be negative.");

        PlayCount += count;
    }

    public override string Describe() =>
        $"{Card()} [Plays: {PlayCount}]";
}