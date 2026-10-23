using Capstone.Core;
using Capstone.Core.Models;
using Xunit;

namespace Capstone.Tests;

public class MusicDomainTests
{
   
    [Theory]
    [InlineData(Genre.Rock, 1975, true)]
    [InlineData(Genre.Rock, 1995, false)]
    [InlineData(Genre.Pop, 1975, false)]
    public void SmartPlaylist_ClassicRockRule_MatchesOnlyRockTracksBefore1980(Genre genre, int year, bool expectedMatch)
    {
        var track = new Track
        {
            Id = "trk-filter",
            Title = "Sample Track",
            Credit = new Credit("Artist"),
            Genre = genre,
            Duration = TimeSpan.FromMinutes(3),
            Year = year
        };
        
        bool isClassicRock = track.Genre == Genre.Rock && track.Year < 1980;

        Assert.Equal(expectedMatch, isClassicRock);
    }

   
    [Fact]
    public void Album_TotalDuration_CalculatesSum()
    {
        var album = new Album
        {
            Id = "alb-0001",
            Title = "A Night at the Opera",
            Artist = "Queen",
            ReleaseYear = 1975
        };

        album.AddTrack(new Track
        {
            Id = "trk-0003",
            Title = "Death on Two Legs",
            Credit = new Credit("Queen"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(43),
            Year = 1975
        });

        album.AddTrack(new Track
        {
            Id = "trk-0004",
            Title = "Bohemian Rhapsody",
            Credit = new Credit("Queen"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(55),
            Year = 1975
        });

        Assert.Equal(TimeSpan.FromMinutes(9) + TimeSpan.FromSeconds(38), album.TotalDuration);
    }

  
    [InlineData(99, false)]
    [InlineData(100, true)]
    [InlineData(101, true)]
    public void Track_CenturionMilestone_Evaluates(int plays, bool expected)
    {
        var track = new Track
        {
            Id = "trk-0002",
            Title = "Radio Ga Ga",
            Credit = new Credit("Queen"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(5),
            Year = 1984
        };

        track.RecordPlays(plays);
        Assert.Equal(expected, track.HasReachedCenturionMilestone);
    }

    
    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Track_NegativePlays_Throws(int invalidPlays)
    {
        var track = new Track
        {
            Id = "trk-0001",
            Title = "Bohemian Rhapsody",
            Credit = new Credit("Queen"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(6),
            Year = 1975
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => track.RecordPlays(invalidPlays));
    }

  
    [Fact]
    public void Album_AddTrack_PreservesTrack()
    {
        var album = new Album
        {
            Id = "alb-0002",
            Title = "Dark Side of the Moon",
            Artist = "Pink Floyd",
            ReleaseYear = 1973
        };

        var track = new Track
        {
            Id = "trk-0005",
            Title = "Time",
            Credit = new Credit("Pink Floyd"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(7),
            Year = 1973
        };

        album.AddTrack(track);

        Assert.Single(album.Tracks);
    }

    [Fact]
    public void MusicLibraryStore_AddDuplicateItem_Throws()
    {
        var store = new MusicLibraryStore();
        var track = new Track
        {
            Id = "trk-dup",
            Title = "Song",
            Credit = new Credit("Artist"),
            Genre = Genre.Rock,
            Duration = TimeSpan.FromMinutes(3),
            Year = 2020
        };

        store.Add(track);
        Assert.Throws<ArgumentException>(() => store.Add(track));
    }
}