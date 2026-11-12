using System;
using Capstone.Core;
using Capstone.Core.Models;
using Capstone.Core.Wiring;
using SharpQuest.Contracts;

Console.WriteLine("Music Library: Event Pipeline, Indexing & Higher-Order Functions\n");

var store = new MusicLibraryStore();
var wiring = new Lab06Wiring();

Console.WriteLine("--- 1. Subscribing Listeners ---");

int totalAdded = 0;
void CounterSubscriber(IItem item)
{
    totalAdded++;
    Console.WriteLine($"  [Event -> Counter] New item added: \"{item.DisplayName}\" | Total count: {totalAdded}");
}


var recentHistory = new RecentlyUsed<IItem>(capacity: 2);
void HistorySubscriber(IItem item)
{
    recentHistory.Push(item);
    Console.WriteLine($"  [Event -> History] Stored in recent cache: \"{item.DisplayName}\"");
}

store.ItemAdded += CounterSubscriber;
store.ItemAdded += HistorySubscriber;

Console.WriteLine("\nAdding Track 1...");
var track1 = new StudioTrack
{
    Id = "trk-001",
    Title = "Bohemian Rhapsody",
    Credit = new Credit("Queen"),
    Genre = Genre.Rock,
    Duration = TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(55),
    Year = 1975,
    AlbumTitle = "A Night at the Opera"
};
store.Add(track1);


Console.WriteLine("\nAdding Track 2...");
var track2 = new LiveTrack
{
    Id = "trk-002",
    Title = "Hotel California",
    Credit = new Credit("Eagles"),
    Genre = Genre.Rock,
    Duration = TimeSpan.FromMinutes(6) + TimeSpan.FromSeconds(30),
    Year = 1976,
    Venue = "The Forum, LA"
};
store.Add(track2);


Console.WriteLine("\n--- 2. Unsubscribing Counter ---");
store.ItemAdded -= CounterSubscriber;

Console.WriteLine("Adding Track 3 (only HistorySubscriber should trigger)...");
var track3 = new StudioTrack
{
    Id = "trk-003",
    Title = "Come Together",
    Credit = new Credit("The Beatles"),
    Genre = Genre.Rock,
    Duration = TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(20),
    Year = 1969,
    AlbumTitle = "Abbey Road"
};
store.Add(track3);

//LAB 6
Console.WriteLine("\n--- 3. Testing Lab06Wiring Functions ---");

// Where: Filter tracks by rock genre
var rockTracks = wiring.Where(store, item => item is PlayableItem p && p.Genre == Genre.Rock);
Console.WriteLine($"[Wiring.Where] Rock tracks matching predicate: {rockTracks.Count}");

// Map: Project items into formatted description strings
var descriptions = wiring.Map(store, item => $"[ID: {item.Id}] {item.DisplayName}");
Console.WriteLine("[Wiring.Map] Generated text projections:");
foreach (var desc in descriptions)
{
    Console.WriteLine($"  * {desc}");
}

// ForEach: Execute an action on each element
Console.WriteLine("[Wiring.ForEach] Iterating items with custom action:");
wiring.ForEach(store, item => Console.WriteLine($"  -> Action ran on: {item.DisplayName}"));

//LAB 5
Console.WriteLine("\n--- 4. Testing Lab 05 Features ---");

//index
var firstItem = store[0];
Console.WriteLine($"store[0] (numeric indexer): \"{firstItem.DisplayName}\" (ID: {firstItem.Id})");

// id
var foundItem = store["trk-002"];
Console.WriteLine($"store[\"trk-002\"] (ID indexer): \"{foundItem?.DisplayName}\"");

var missingItem = store["trk-999"];
Console.WriteLine($"store[\"trk-999\"] (missing ID): {(missingItem == null ? "null (correct)" : "error")}");

Console.WriteLine("\nTraversing store via direct foreach:");
foreach (var item in store)
{
    Console.WriteLine($"  * {item.DisplayName} ({item.Id})");
}

Console.WriteLine("\nRunning deferred search Matching(\"Hotel\"):");
foreach (var match in store.Matching("Hotel"))
{
    Console.WriteLine($"  * Match: \"{match.DisplayName}\"");
}

Console.WriteLine("\nRecentlyUsed<IItem> cache contents (capacity 2):");
foreach (var recent in recentHistory)
{
    Console.WriteLine($"  * In Buffer: \"{recent.DisplayName}\"");
}

//LAB7
Console.WriteLine("\n 5. Testing Lab 07 LINQ Queries");
var lab07 = new Lab07Wiring();


var lazyNames = lab07.Names(store);
Console.WriteLine($"[Names] Deferred count: {lazyNames.Count()}");


Console.WriteLine("[Sorted] Items in alphabetical order:");
foreach (var item in lab07.Sorted(store))
{
    Console.WriteLine($"  * {item.DisplayName} ({item.Id})");
}

Console.WriteLine("[Longest(2)] Top 2 longest display names:");
foreach (var item in store.Longest(2))
{
    Console.WriteLine($"  * {item.DisplayName} ({item.DisplayName.Length} chars)");
}

var stats = lab07.Summary(store);
Console.WriteLine($"[Summary] {stats}");

//Queries
Console.WriteLine("\nMusic Library Domain Queries");
Console.WriteLine($"1. Top artist: {store.TopArtistByTrackCount() ?? "None"}");

Console.WriteLine("2. Minutes per genre:");
foreach (var (genre, mins) in store.MinutesPerGenre())
{
    Console.WriteLine($"   - {genre}: {mins} min");
}

var longestAlbum = store.LongestStudioAlbum();
if (longestAlbum is { } album)
{
    Console.WriteLine($"3. Longest studio album: \"{album.AlbumTitle}\" ({album.TotalDuration})");
}

var topDecade = store.TopDecade();
Console.WriteLine($"4. Dominant decade: {(topDecade.HasValue ? $"{topDecade}s" : "N/A")}");

Console.WriteLine("5. Live performances >= 5 min:");
foreach (var live in store.LongLivePerformances(TimeSpan.FromMinutes(5)))
{
    Console.WriteLine($"   - \"{live.Title}\" at {live.Venue} ({live.Duration})");
}


Console.WriteLine("\n 6. Lab 07 Stretch Task");

Console.WriteLine($"Is store empty? {store.IsEmpty()}");
Console.WriteLine($"Catalog one-liner: {store.OneLine()}");
Console.WriteLine($"Total catalog runtime: {store.TotalRuntime()}");

var genreDescriptions = new List<(Genre Genre, string MoodNote)>
{
    (Genre.Rock, "High-energy guitar & drums"),
    (Genre.Jazz, "Improvisational & late-night vibes"),
    (Genre.Electronic, "Synth-driven & rhythmic")
};

Console.WriteLine("\n[Join] Tracks matched with genre descriptions:");
foreach (var line in store.MatchTracksWithGenreNotes(genreDescriptions))
{
    Console.WriteLine($"  * {line}");
}

Console.WriteLine("\n[GroupBy] Tracks grouped by Genre (outer sequence = groups, inner sequence = tracks):");
var byGenre = store.OfType<PlayableItem>().GroupBy(t => t.Genre);
foreach (var group in byGenre)
{
    Console.WriteLine($"  Genre: {group.Key} ({group.Count()} tracks)");
    foreach (var track in group)
    {
        Console.WriteLine($"    - {track.Title} ({track.Duration})");
    }
}