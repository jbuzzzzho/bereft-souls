using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using System.Threading.Tasks;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Transforms;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

using JetBrains.Annotations;

using Mono.Cecil;

using MonoMod.Cil;

using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

using IAssemblyResolver = ICSharpCode.Decompiler.Metadata.IAssemblyResolver;

namespace EditDecompiler;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
public sealed class TheMod : Mod;

public static class TheDecompiler
{
    private sealed class AssemblyResolver : IAssemblyResolver
    {
        private readonly Dictionary<string, List<Assembly>> cache = [];

        public AssemblyResolver()
        {
            var modules = AssemblyLoadContext.All.SelectMany(x => x.Assemblies).DistinctBy(x => x.ManifestModule.FullyQualifiedName).Select(x => x.ManifestModule);
            foreach (var module in modules)
            {
                if (!cache.TryGetValue(module.Assembly.GetName().Name!, out var assemblies))
                {
                    assemblies = [];
                    cache.Add(module.Assembly.GetName().Name!, assemblies);
                }

                assemblies.Add(module.Assembly);
            }

            /*var assemblies = AssemblyLoadContext.All.SelectMany(x => x.Assemblies).DistinctBy(x => x.GetName().Name);
            assemblies = assemblies.Where(
                x =>
                {
                    try
                    {
                        // TODO: we'll have to write out mod references to disk
                        //       for supporting resolving mods... thanks IL spy
                        return !string.IsNullOrWhiteSpace(x.Location);
                    }
                    catch
                    {
                        return false;
                    }
                }
            );*/
        }

        public MetadataFile? Resolve(IAssemblyReference reference)
        {
            if (!cache.TryGetValue(reference.Name, out var assemblies))
            {
                return null;
            }

            if (assemblies.Count == 1)
            {
                if (assemblies[0].GetName().Version != reference.Version)
                {
                    //
                }

                return MakePeFile(assemblies[0]);
            }

            var highestVersion = default(Assembly);
            var exactMatch     = default(Assembly);

            var publicKeyTokenOfName = reference.PublicKeyToken ?? [];

            foreach (var assembly in assemblies)
            {
                var version        = assembly.GetName().Version;
                var publicKeyToken = assembly.GetName().GetPublicKeyToken() ?? [];

                if (version == reference.Version && publicKeyToken.SequenceEqual(publicKeyTokenOfName))
                {
                    exactMatch = assembly;
                }
                else if (highestVersion is null || highestVersion.GetName().Version < version)
                {
                    highestVersion = assembly;
                }
            }

            var chosen = exactMatch ?? highestVersion;
            return MakePeFile(chosen);

            static PEFile? MakePeFile(Assembly? assembly)
            {
                if (assembly is null)
                {
                    return null;
                }

                try
                {
                    if (!string.IsNullOrEmpty(assembly.Location))
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }

                var path = assembly.Location;
                // using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                return new PEFile(path, PEStreamOptions.PrefetchMetadata);
            }
        }

        public MetadataFile? ResolveModule(MetadataFile mainModule, string moduleName)
        {
            var baseDirectory  = Path.GetDirectoryName(mainModule.FileName)!;
            var moduleFileName = Path.Combine(baseDirectory, moduleName);
            if (!File.Exists(moduleFileName))
            {
                return null;
            }

            return new PEFile(moduleFileName, PEStreamOptions.PrefetchMetadata);
        }

        public Task<MetadataFile?> ResolveAsync(IAssemblyReference reference)
        {
            return Task.FromResult(Resolve(reference));
        }

        public Task<MetadataFile?> ResolveModuleAsync(MetadataFile mainModule, string moduleName)
        {
            return Task.FromResult(ResolveModule(mainModule, moduleName));
        }
    }

    private static readonly Dictionary<ILContext, MetadataFile> metadata_files    = new();
    private static readonly AssemblyResolver                    assembly_resolver = new();

    public static void DecompileAndDump(Mod mod, ILContext il)
    {
        var metadataFile = GetMetadataFileForIlContext(il);

        var decompiler = new CSharpDecompiler(metadataFile, assembly_resolver, new DecompilerSettings());
        decompiler.AstTransforms.Add(new EscapeInvalidIdentifiers());

        // var fullTypeName = new FullTypeName(il.Method.DeclaringType.FullName.Replace('/', '+'));
        var type = decompiler.TypeSystem.MainModule.TypeDefinitions.FirstOrDefault(x => x.FullName == il.Method.DeclaringType.FullName); // .GetTypeDefinition(fullTypeName);
        if (type is null)
        {
            throw new Exception("Failed to find type");
        }

        /*var method = type.Methods.FirstOrDefault(x => x.Name == il.Method.Name);
        if (method is null)
        {
            throw new Exception($"Failed to find method {il.Method.Name}");
        }*/

        var text = decompiler.DecompileTypeAsString(type.FullTypeName);

        var methodName = il.Method.FullName.Replace(':', '_');
        if (methodName.Contains('?'))
        {
            methodName = methodName[(methodName.LastIndexOf('?') + 1)..];
        }

        var filePath   = Path.Combine(Logging.LogDir, "ILDumps", "decompiled", mod.Name, methodName + ".txt");
        var folderPath = Path.GetDirectoryName(filePath);

        Directory.CreateDirectory(folderPath!);
        File.WriteAllText(filePath, text);
    }

    private static MetadataFile GetMetadataFileForIlContext(ILContext il)
    {
        if (metadata_files.TryGetValue(il, out var file))
        {
            return file;
        }

        var tempPath = CreateDirectory(GetTempPath("dll"));
        var stream   = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
        CleanUpIl(il);
        il.Module.Assembly.MainModule.Write(stream, new WriterParameters { WriteSymbols = false });
        stream.Dispose();

        return new PEFile(tempPath, PEStreamOptions.PrefetchEntireImage);
    }

    private static string GetTempPath(string extension)
    {
        return Path.Combine(Main.SavePath, "EditDecompiler", $"{new UnifiedRandom().Next()}.{extension}");
    }

    private static string CreateDirectory(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return path;
    }

    // this breaks the context btw (I think)
    private static void CleanUpIl(ILContext il)
    {
        il.Instrs.Where(x => x.Operand is ILLabel).ToList().ForEach(x => x.Operand = ((ILLabel)x.Operand).Target);
    }
}