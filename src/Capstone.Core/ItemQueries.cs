using System;
using System.Collections.Generic;
using System.Linq;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core;

public static class ItemQueries
{
    public static IEnumerable<IItem> Longest(this IEnumerable<IItem> items, int n)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return items.OrderByDescending(i => i.DisplayName.Length)
                    .ThenBy(i => i.Id, StringComparer.Ordinal)
                    .Take(n);
    }

    public static string? TopArtistByTrackCount(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items.OfType<PlayableItem>()
            .GroupBy(track => track.Credit.ToString() ?? "Unknown", StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Key)
            .FirstOrDefault();
    }
    public static IEnumerable<(Genre Genre, double TotalMinutes)> MinutesPerGenre(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items.OfType<PlayableItem>()
                    .GroupBy(track => track.Genre)
                    .Select(group => (
                        Genre: group.Key,
                        TotalMinutes: Math.Round(group.Sum(t => t.Duration.TotalMinutes), 2)
                    ))
                    .OrderByDescending(entry => entry.TotalMinutes);
    }

    public static (string AlbumTitle, TimeSpan TotalDuration)? LongestStudioAlbum(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items.OfType<StudioTrack>()
                    .Where(track => !string.IsNullOrWhiteSpace(track.AlbumTitle))
                    .GroupBy(track => track.AlbumTitle, StringComparer.OrdinalIgnoreCase)
                    .Select(group => (
                        AlbumTitle: group.Key,
                        TotalDuration: TimeSpan.FromTicks(group.Sum(t => t.Duration.Ticks))
                    ))
                    .OrderByDescending(album => album.TotalDuration)
                    .Select(album => ((string, TimeSpan)?)album)
                    .FirstOrDefault();
    }

    public static int? TopDecade(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items.OfType<PlayableItem>()
                    .Where(track => track.Year > 0)
                    .GroupBy(track => (track.Year / 10) * 10)
                    .OrderByDescending(group => group.Count())
                    .ThenByDescending(group => group.Key)
                    .Select(group => (int?)group.Key)
                    .FirstOrDefault();
    }

    public static IEnumerable<LiveTrack> LongLivePerformances(this IEnumerable<IItem> items, TimeSpan minDuration)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items.OfType<LiveTrack>()
                    .Where(live => live.Duration >= minDuration)
                    .OrderByDescending(live => live.Duration)
                    .ThenBy(live => live.Title, StringComparer.OrdinalIgnoreCase);
    }
    
    
    //strech
    public static IEnumerable<string> MatchTracksWithGenreNotes(
        this IEnumerable<IItem> items,
        IEnumerable<(Genre Genre, string MoodNote)> genreNotes)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(genreNotes);

        return items.OfType<PlayableItem>()
            .Join(
                inner: genreNotes,
                outerKeySelector: track => track.Genre,
                innerKeySelector: note => note.Genre,
                resultSelector: (track, note) => $"\"{track.Title}\" [{track.Genre}] -> {note.MoodNote}"
            );
    }

  
    public static string OneLine(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return string.Join(", ", items.Select(i => i.DisplayName));
    }

    public static bool IsEmpty(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return !items.Any();
    }

    public static TimeSpan TotalRuntime(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        long totalTicks = items.OfType<PlayableItem>().Sum(t => t.Duration.Ticks);
        return TimeSpan.FromTicks(totalTicks);
    }
}