namespace Capstone.Core.Models;

public interface IPlayable
{
    TimeSpan Duration { get; }
    int PlayCount { get; }
    void RecordPlay();
}