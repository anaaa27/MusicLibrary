using System.Text.Json;
using Capstone.Core.Models;
using SharpQuest.Contracts;

namespace Capstone.Core;

public record TrackDto(
    string? Id,
    string? Title,
    string? Artist,
    string? FeaturedArtist = null,
    string? Genre = "Rock",
    string? Duration = "00:03:30",
    int Year = 2024,
    int? Rating = null,
    string? DisplayName = null,
    string? Kind = null,
    string? AlbumTitle = null,
    string? Venue = null);

public class MusicLibraryMap : IItemMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static TrackDto ToDto(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item switch
        {
            StudioTrack st => new TrackDto(
                st.Id,
                st.Title,
                st.Credit.Artist,
                st.Credit.FeaturedArtist,
                st.Genre.ToString(),
                st.Duration.ToString(),
                st.Year,
                null,
                st.DisplayName,
                nameof(StudioTrack),
                st.AlbumTitle,
                null),

            LiveTrack lt => new TrackDto(
                lt.Id,
                lt.Title,
                lt.Credit.Artist,
                lt.Credit.FeaturedArtist,
                lt.Genre.ToString(),
                lt.Duration.ToString(),
                lt.Year,
                null,
                lt.DisplayName,
                nameof(LiveTrack),
                null,
                lt.Venue),

            Track t => new TrackDto(
                t.Id,
                t.Title,
                t.Credit.Artist,
                t.Credit.FeaturedArtist,
                t.Genre.ToString(),
                t.Duration.ToString(),
                t.Year,
                t.Rating,
                t.DisplayName,
                nameof(Track),
                null,
                null),

            PlayableItem p => new TrackDto(
                p.Id,
                p.Title,
                p.Credit.Artist,
                p.Credit.FeaturedArtist,
                p.Genre.ToString(),
                p.Duration.ToString(),
                p.Year,
                null,
                p.DisplayName,
                p.GetType().Name,
                null,
                null),

            _ => new TrackDto(item.Id, item.DisplayName, "Unknown")
        };
    }

    public static IItem? FromDto(TrackDto? dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
            return null;

        string title = !string.IsNullOrWhiteSpace(dto.Title)
            ? dto.Title
            : dto.DisplayName ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
            return null;

        string artist = !string.IsNullOrWhiteSpace(dto.Artist)
            ? dto.Artist
            : "Unknown Artist";

        if (!Enum.TryParse<Genre>(dto.Genre, true, out var genre))
            return null;

        if (!TimeSpan.TryParse(dto.Duration, out var duration))
            return null;

        var credit = new Credit(artist, dto.FeaturedArtist);

        if (string.Equals(dto.Kind, nameof(StudioTrack), StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(dto.AlbumTitle) && string.IsNullOrWhiteSpace(dto.Kind)))
        {
            if (string.IsNullOrWhiteSpace(dto.AlbumTitle))
                return null;

            return new StudioTrack
            {
                Id = dto.Id,
                Title = title,
                Credit = credit,
                Genre = genre,
                Duration = duration,
                Year = dto.Year,
                AlbumTitle = dto.AlbumTitle
            };
        }

        if (string.Equals(dto.Kind, nameof(LiveTrack), StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(dto.Venue) && string.IsNullOrWhiteSpace(dto.Kind)))
        {
            if (string.IsNullOrWhiteSpace(dto.Venue))
                return null;

            return new LiveTrack
            {
                Id = dto.Id,
                Title = title,
                Credit = credit,
                Genre = genre,
                Duration = duration,
                Year = dto.Year,
                Venue = dto.Venue
            };
        }

        if (dto.Rating is < 1 or > 5)
            return null;

        git add .        return new Track
        {
            Id = dto.Id,
            Title = title,
            Credit = credit,
            Genre = genre,
            Duration = duration,
            Year = dto.Year,
            Rating = dto.Rating ?? 5
        };
    }

    public string ToJson(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return JsonSerializer.Serialize(ToDto(item), JsonOptions);
    }

    public IItem? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var dto = JsonSerializer.Deserialize<TrackDto>(json, JsonOptions);
            return FromDto(dto);
        }
        catch (Exception)
        {
            return null;
        }
    }
}