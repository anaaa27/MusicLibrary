using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text.Json.Nodes;

namespace SharpQuest.Contracts.Tests.Lab03;

/// <summary>The mutation results sq wrote to .sq/mutation/Lab03.json before the tests ran.</summary>
public sealed record MutationResults(string? TestAssembly, int OwnTotal, int OwnFailed, int SpecTotal, int SpecFailed, Dictionary<int, bool> Caught)
{
    public static string RepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "SharpQuest.slnx"))) return dir.FullName;
        throw new InvalidOperationException("Can't find SharpQuest.slnx above " + AppContext.BaseDirectory);
    }

    /// <summary>Loads the results, or skips the calling test when sq hasn't produced them (e.g. a run from the IDE).</summary>
    public static MutationResults LoadOrSkip()
    {
        var file = Path.Combine(RepoRoot(), ".sq", "mutation", "Lab03.json");
        if (!File.Exists(file))
            Assert.Skip("Run `sq test` (not plain dotnet test): it runs your tests against the broken versions first.");
        var j = JsonNode.Parse(File.ReadAllText(file))!;
        var caught = (j["mutants"]?.AsObject() ?? [])
            .ToDictionary(kv => int.Parse(kv.Key), kv => (bool?)kv.Value?["caught"] == true);
        return new MutationResults((string?)j["testAssembly"], (int?)j["ownTotal"] ?? 0, (int?)j["ownFailed"] ?? 0,
            (int?)j["specTotal"] ?? 0, (int?)j["specFailed"] ?? 0, caught);
    }

    /// <summary>The compiled Capstone.Tests.dll: from the mutation results, or the newest one under tests/.</summary>
    public static string? FindTestAssembly()
    {
        var root = RepoRoot();
        var file = Path.Combine(root, ".sq", "mutation", "Lab03.json");
        if (File.Exists(file) && (string?)JsonNode.Parse(File.ReadAllText(file))?["testAssembly"] is { } p && File.Exists(p))
            return p;
        var bin = Path.Combine(root, "tests", "Capstone.Tests", "bin");
        return Directory.Exists(bin)
            ? Directory.EnumerateFiles(bin, "Capstone.Tests.dll", SearchOption.AllDirectories)
                .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault()
            : null;
    }
}

/// <summary>What one of the student's test methods does, read from the compiled IL (nothing is executed).</summary>
public sealed class TestMethodFacts
{
    public required string Class { get; init; }
    public required string Name { get; init; }
    public bool IsTheory { get; set; }
    public int DataRows { get; set; }
    public bool HasDataSource { get; set; }
    public bool UsesOwnCode { get; set; }
    public bool UsesSearchMatcher { get; set; }
    public bool ChecksException { get; set; }
    public override string ToString() => $"{Class}.{Name}";
}

/// <summary>
/// Reads the student's test assembly with System.Reflection.Metadata: which methods are tests,
/// what they call (following lambdas, async bodies and local helpers), and whether anything peeks at the grader.
/// </summary>
public sealed class TestAssemblyFacts
{
    public List<TestMethodFacts> Tests { get; } = [];
    public List<string> Peeking { get; } = [];

    private static readonly Dictionary<ushort, OpCode> OpCodesByValue =
        typeof(OpCodes).GetFields().Select(f => (OpCode)f.GetValue(null)!).ToDictionary(o => (ushort)o.Value);

    private static readonly HashSet<string> ReflectionOnType =
        ["GetField", "GetFields", "GetProperty", "GetProperties", "GetMethod", "GetMethods", "GetMember", "GetMembers", "InvokeMember", "GetTypeInfo", "GetRuntimeFields"];

    public static TestAssemblyFacts Load(string path)
    {
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream);
        var md = pe.GetMetadataReader();
        var facts = new TestAssemblyFacts();
        var walker = new Walker(md, pe);

        foreach (var th in md.TypeDefinitions)
        {
            var type = md.GetTypeDefinition(th);
            var className = md.GetString(type.Name);
            var isSpecClass = className.Contains("SearchMatcher", StringComparison.OrdinalIgnoreCase);

            if (isSpecClass)
            {
                // Everything in a spec class counts for peeking, including constructors and static initialisers.
                foreach (var mh in type.GetMethods())
                    foreach (var p in walker.Scan(mh).Peeks) facts.Peeking.Add($"{className}: {p}");
                foreach (var nested in type.GetNestedTypes())
                    foreach (var mh in md.GetTypeDefinition(nested).GetMethods())
                        foreach (var p in walker.Scan(mh).Peeks) facts.Peeking.Add($"{className}: {p}");
            }

            foreach (var mh in type.GetMethods())
            {
                var method = md.GetMethodDefinition(mh);
                var name = md.GetString(method.Name);
                TestMethodFacts? test = null;
                foreach (var ch in method.GetCustomAttributes())
                {
                    var attr = walker.AttributeName(md.GetCustomAttribute(ch));
                    if (attr is "FactAttribute" or "TheoryAttribute")
                        test ??= new TestMethodFacts { Class = className, Name = name };
                    if (attr == "TheoryAttribute") test!.IsTheory = true;
                    if (attr == "InlineDataAttribute") (test ??= new TestMethodFacts { Class = className, Name = name }).DataRows++;
                    if (attr is "MemberDataAttribute" or "ClassDataAttribute") (test ??= new TestMethodFacts { Class = className, Name = name }).HasDataSource = true;
                }
                if (test is null || !(test.IsTheory || IsFact(walker, method))) continue;

                // The test body, plus compiler-generated bodies that belong to it (lambdas, async state machines).
                var scan = walker.Scan(mh);
                foreach (var related in RelatedBodies(md, th, name))
                    scan = scan.Merge(walker.Scan(related));

                test.UsesOwnCode = scan.OwnCode;
                test.UsesSearchMatcher = scan.SearchMatcher;
                test.ChecksException = scan.ExceptionCheck;
                facts.Tests.Add(test);
            }
        }
        return facts;
    }

    private static bool IsFact(Walker w, MethodDefinition m) =>
        m.GetCustomAttributes().Any(ch => w.AttributeName(w.Md.GetCustomAttribute(ch)) == "FactAttribute");

    private static IEnumerable<MethodDefinitionHandle> RelatedBodies(MetadataReader md, TypeDefinitionHandle th, string methodName)
    {
        var marker = $"<{methodName}>";
        var stack = new Stack<TypeDefinitionHandle>([th]);
        while (stack.Count > 0)
        {
            var t = md.GetTypeDefinition(stack.Pop());
            var typeName = md.GetString(t.Name);
            foreach (var mh in t.GetMethods())
            {
                var n = md.GetString(md.GetMethodDefinition(mh).Name);
                if (n.Contains(marker) || (typeName.Contains(marker) && n == "MoveNext")) yield return mh;
            }
            foreach (var nested in t.GetNestedTypes()) stack.Push(nested);
        }
    }

    public sealed record Scan(bool OwnCode, bool SearchMatcher, bool ExceptionCheck, List<string> Peeks)
    {
        public Scan Merge(Scan o) => new(OwnCode || o.OwnCode, SearchMatcher || o.SearchMatcher, ExceptionCheck || o.ExceptionCheck, [.. Peeks, .. o.Peeks]);
    }

    private sealed class Walker(MetadataReader md, PEReader pe)
    {
        public MetadataReader Md => md;
        private readonly Dictionary<MethodDefinitionHandle, Scan> _cache = [];
        private readonly HashSet<MethodDefinitionHandle> _inProgress = [];

        public string AttributeName(CustomAttribute ca) => ca.Constructor.Kind switch
        {
            HandleKind.MethodDefinition => md.GetString(md.GetTypeDefinition(md.GetMethodDefinition((MethodDefinitionHandle)ca.Constructor).GetDeclaringType()).Name),
            HandleKind.MemberReference => TypeOf(md.GetMemberReference((MemberReferenceHandle)ca.Constructor).Parent).Name,
            _ => "",
        };

        public Scan Scan(MethodDefinitionHandle mh)
        {
            if (_cache.TryGetValue(mh, out var cached)) return cached;
            if (!_inProgress.Add(mh)) return new(false, false, false, []);
            var result = new Scan(false, false, false, []);
            var method = md.GetMethodDefinition(mh);
            if (method.RelativeVirtualAddress != 0)
            {
                var il = pe.GetMethodBody(method.RelativeVirtualAddress).GetILReader();
                while (il.RemainingBytes > 0)
                {
                    int code = il.ReadByte();
                    if (code == 0xFE) code = 0xFE00 | il.ReadByte();
                    if (!OpCodesByValue.TryGetValue((ushort)code, out var op)) break;
                    switch (op.OperandType)
                    {
                        case OperandType.InlineNone: break;
                        case OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar: il.ReadByte(); break;
                        case OperandType.InlineVar: il.ReadUInt16(); break;
                        case OperandType.InlineI or OperandType.InlineBrTarget or OperandType.InlineSig: il.ReadInt32(); break;
                        case OperandType.ShortInlineR: il.ReadSingle(); break;
                        case OperandType.InlineR: il.ReadDouble(); break;
                        case OperandType.InlineI8: il.ReadInt64(); break;
                        case OperandType.InlineSwitch: var n = il.ReadInt32(); for (var i = 0; i < n; i++) il.ReadInt32(); break;
                        case OperandType.InlineString:
                            var s = md.GetUserString(MetadataTokens.UserStringHandle(il.ReadInt32() & 0xFFFFFF));
                            if (s.Contains("SQ_MUTANT", StringComparison.OrdinalIgnoreCase)) result.Peeks.Add("mentions SQ_MUTANT");
                            break;
                        default:
                            result = result.Merge(Entity(MetadataTokens.EntityHandle(il.ReadInt32())));
                            break;
                    }
                }
            }
            _inProgress.Remove(mh);
            return _cache[mh] = result;
        }

        private Scan Entity(EntityHandle h)
        {
            switch (h.Kind)
            {
                case HandleKind.MethodDefinition:
                {
                    // A method in the test assembly itself: a helper, so follow it.
                    var def = md.GetMethodDefinition((MethodDefinitionHandle)h);
                    var owner = TypeOf(def.GetDeclaringType());
                    var name = md.GetString(def.Name);
                    var checksException = (owner.Name == "Assert" && name.StartsWith("Throws")) || (owner.Name == "Record" && name.StartsWith("Exception"));
                    return Scan((MethodDefinitionHandle)h).Merge(new Scan(false, false, checksException, []));
                }
                case HandleKind.MethodSpecification:
                    var spec = md.GetMethodSpecification((MethodSpecificationHandle)h);
                    var args = spec.DecodeSignature(new Names(), null);
                    return Entity(spec.Method).Merge(FromTypeNames(args));
                case HandleKind.MemberReference:
                    var mr = md.GetMemberReference((MemberReferenceHandle)h);
                    var parent = TypeOf(mr.Parent);
                    var member = md.GetString(mr.Name);
                    var scan = FromType(parent);
                    var peeks = new List<string>();
                    if (parent.Full == "System.Environment" && member.StartsWith("GetEnvironmentVariable")) peeks.Add("reads environment variables");
                    if (parent.Full == "System.Type" && ReflectionOnType.Contains(member)) peeks.Add($"uses reflection (Type.{member})");
                    var isExceptionCheck = (parent.Name == "Assert" && member.StartsWith("Throws")) || (parent.Name == "Record" && member.StartsWith("Exception"));
                    return scan.Merge(new Scan(false, false, isExceptionCheck, peeks));
                case HandleKind.FieldDefinition:
                    return new(false, false, false, []);
                case HandleKind.TypeReference or HandleKind.TypeDefinition or HandleKind.TypeSpecification:
                    return FromType(TypeOf(h));
                default:
                    return new(false, false, false, []);
            }
        }

        private static Scan FromTypeNames(IEnumerable<string> names) =>
            names.Select(n => new Scan(IsOwn(n), n.Contains("SharpQuest.Contracts.SearchMatcher"), false, []))
                 .Aggregate(new Scan(false, false, false, []), (a, b) => a.Merge(b));

        private static Scan FromType(TypeName t)
        {
            var peeks = t.Full.StartsWith("System.Reflection") && !t.Full.StartsWith("System.Reflection.Metadata")
                ? new List<string> { $"uses reflection ({t.Full})" } : [];
            return new Scan(IsOwn(t.Assembly + "!" + t.Full) || t.GenericArgs.Any(IsOwn),
                            t.Full == "SharpQuest.Contracts.SearchMatcher" || t.GenericArgs.Any(a => a.Contains("SharpQuest.Contracts.SearchMatcher")),
                            false, peeks);
        }

        /// <summary>Your own code: anything from Capstone.Core except the wiring classes.</summary>
        private static bool IsOwn(string qualified) =>
            qualified.Contains("Capstone.Core!") && !qualified.Contains("Capstone.Core!Capstone.Core.Wiring");

        public TypeName TypeOf(EntityHandle h)
        {
            switch (h.Kind)
            {
                case HandleKind.TypeReference:
                {
                    var tr = md.GetTypeReference((TypeReferenceHandle)h);
                    var name = md.GetString(tr.Name);
                    var ns = md.GetString(tr.Namespace);
                    var scope = tr.ResolutionScope;
                    while (scope.Kind == HandleKind.TypeReference)   // nested type: climb to the outer one
                    {
                        var outer = md.GetTypeReference((TypeReferenceHandle)scope);
                        ns = md.GetString(outer.Namespace);
                        scope = outer.ResolutionScope;
                    }
                    var asm = scope.Kind == HandleKind.AssemblyReference
                        ? md.GetString(md.GetAssemblyReference((AssemblyReferenceHandle)scope).Name) : "";
                    return new TypeName(asm, ns, name, []);
                }
                case HandleKind.TypeDefinition:
                {
                    var td = md.GetTypeDefinition((TypeDefinitionHandle)h);
                    return new TypeName("", md.GetString(td.Namespace), md.GetString(td.Name), []);
                }
                case HandleKind.TypeSpecification:
                {
                    var decoded = md.GetTypeSpecification((TypeSpecificationHandle)h).DecodeSignature(new Names(), null);
                    // "asm!ns.Name`1<asm!arg,...>" → the generic definition plus its arguments
                    var lt = decoded.IndexOf('<');
                    var head = lt < 0 ? decoded : decoded[..lt];
                    var args = lt < 0 ? [] : decoded[(lt + 1)..^1].Split(',').ToList();
                    var bang = head.IndexOf('!');
                    var full = bang < 0 ? head : head[(bang + 1)..];
                    var dot = full.LastIndexOf('.');
                    return new TypeName(bang < 0 ? "" : head[..bang], dot < 0 ? "" : full[..dot], dot < 0 ? full : full[(dot + 1)..], args) { Raw = decoded };
                }
                case HandleKind.MethodDefinition:
                    return TypeOf(md.GetMethodDefinition((MethodDefinitionHandle)h).GetDeclaringType());
                default:
                    return new TypeName("", "", "", []);
            }
        }
    }

    public sealed record TypeName(string Assembly, string Namespace, string Name, List<string> GenericArgs)
    {
        public string Full => Namespace.Length == 0 ? Name : $"{Namespace}.{Name}";
        public string? Raw { get; init; }
    }

    /// <summary>Turns signature types into "assembly!namespace.Name" strings.</summary>
    private sealed class Names : ISignatureTypeProvider<string, object?>
    {
        public string GetPrimitiveType(PrimitiveTypeCode typeCode) => "System!" + typeCode;
        public string GetTypeFromDefinition(MetadataReader r, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            var t = r.GetTypeDefinition(handle);
            return "!" + r.GetString(t.Namespace) + "." + r.GetString(t.Name);
        }
        public string GetTypeFromReference(MetadataReader r, TypeReferenceHandle handle, byte rawTypeKind)
        {
            var t = r.GetTypeReference(handle);
            var ns = r.GetString(t.Namespace);
            var scope = t.ResolutionScope;
            while (scope.Kind == HandleKind.TypeReference)
            {
                var outer = r.GetTypeReference((TypeReferenceHandle)scope);
                ns = r.GetString(outer.Namespace);
                scope = outer.ResolutionScope;
            }
            var asm = scope.Kind == HandleKind.AssemblyReference ? r.GetString(r.GetAssemblyReference((AssemblyReferenceHandle)scope).Name) : "";
            return $"{asm}!{ns}.{r.GetString(t.Name)}";
        }
        public string GetTypeFromSpecification(MetadataReader r, object? ctx, TypeSpecificationHandle handle, byte rawTypeKind) =>
            r.GetTypeSpecification(handle).DecodeSignature(this, ctx);
        public string GetSZArrayType(string elementType) => elementType;
        public string GetArrayType(string elementType, ArrayShape shape) => elementType;
        public string GetByReferenceType(string elementType) => elementType;
        public string GetPointerType(string elementType) => elementType;
        public string GetPinnedType(string elementType) => elementType;
        public string GetGenericInstantiation(string genericType, System.Collections.Immutable.ImmutableArray<string> typeArguments) =>
            genericType + "<" + string.Join(",", typeArguments) + ">";
        public string GetGenericTypeParameter(object? ctx, int index) => "!T" + index;
        public string GetGenericMethodParameter(object? ctx, int index) => "!M" + index;
        public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
    }
}
