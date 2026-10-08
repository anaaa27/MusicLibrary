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
    string? DisplayName = null);



public class MusicLibraryMap : IItemMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string ToJson(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        TrackDto dto = item switch
        {
            Track t => new TrackDto(
                t.Id,
                t.Title,
                t.Credit.Artist,
                t.Credit.FeaturedArtist,
                t.Genre.ToString(),
                t.Duration.ToString(),
                t.Year,
                t.Rating,
                t.DisplayName),

            _ => new TrackDto(item.Id, item.DisplayName, "Unknown")
        };

        return JsonSerializer.Serialize(dto, JsonOptions);
    }

    public IItem? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var dto = JsonSerializer.Deserialize<TrackDto>(json, JsonOptions);
            if (dto is null)
                return null;

            if (string.IsNullOrWhiteSpace(dto.Id))
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

            if (dto.Rating is < 1 or > 5)
                return null;

            return new Track
            {
                Id = dto.Id,
                Title = title,
                Credit = new Credit(artist, dto.FeaturedArtist),
                Genre = genre,
                Duration = duration,
                Year = dto.Year,
                Rating = dto.Rating
            };
        }
        catch (Exception)
        {
            return null;
        }
    }
}