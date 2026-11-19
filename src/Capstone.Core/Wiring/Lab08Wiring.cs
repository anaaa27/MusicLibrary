using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab08Wiring : ILab08Wiring
{
    private readonly Lab02Wiring _lab02 = new();
    private readonly Lab04Wiring _lab04 = new();

    public IItemStore CreateStore() => _lab02.CreateStore();

    public IItem CreateSample(int seed) => _lab02.CreateSample(seed);

    public IReadOnlyList<Type> KindTypes => _lab04.KindTypes;

    public IItem CreateOfKind(int kindIndex, int seed) => _lab04.CreateOfKind(kindIndex, seed);

    public Task SaveAsync(IItemStore store, string path) => MusicLibraryFile.SaveAsync(store, path);

    public Task<IItemStore> LoadAsync(string path) => MusicLibraryFile.LoadAsync(path, CreateStore);
}