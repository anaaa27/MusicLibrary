using System.Text.Json;
using SharpQuest.Contracts;

namespace Capstone.Core.Storage;

public static class MusicLibraryFile
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static async Task SaveAsync(IItemStore store, string path)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string fullPath = Path.GetFullPath(path);
        if (Path.GetDirectoryName(fullPath) is { Length: > 0 } folder)
        {
            Directory.CreateDirectory(folder);
        }

        var rows = store.All()
                        .Select(MusicLibraryMap.ToDto)
                        .ToList();

        //stretch 
        string tempPath = fullPath + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, rows, Options);
        }

        File.Move(tempPath, fullPath, overwrite: true);
    }
    
    public static async Task<IItemStore> LoadAsync(string path, Func<IItemStore> storeFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(storeFactory);

        if (!File.Exists(path))
        {
            return storeFactory();
        }

        List<TrackDto>? rows;
        try
        {
            await using var stream = File.OpenRead(path);
            if (stream.Length == 0)
            {
                throw new InvalidDataException($"Save file '{path}' is empty.");
            }

            rows = await JsonSerializer.DeserializeAsync<List<TrackDto>>(stream, Options);
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or IOException)
        {
            throw new InvalidDataException($"Save file '{path}' is corrupt or unreadable.", ex);
        }

        if (rows is null)
        {
            throw new InvalidDataException($"Save file '{path}' deserialized to null.");
        }

        var store = storeFactory();
        foreach (var row in rows)
        {
            var item = MusicLibraryMap.FromDto(row);
            if (item is null)
            {
                throw new InvalidDataException($"Save file '{path}' contains an invalid or corrupt track entry.");
            }

            store.Add(item);
        }

        return store;
    }
}