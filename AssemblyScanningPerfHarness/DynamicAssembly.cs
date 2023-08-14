using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using Mono.Cecil;

namespace AssemblyScanningPerfHarness;

[DebuggerDisplay("Name = {Name}, DynamicName = {DynamicName}, Namespace = {Namespace}, FileName = {FileName}")]
class DynamicAssembly
{
    public DynamicAssembly(string nameWithoutExtension, string? assemblyDirectory, DynamicAssembly[]? references = null, Version? version = null,
        bool fakeIdentity = false, string? content = null, bool executable = false)
    {
        version ??= new Version(1, 0, 0, 0);
        assemblyDirectory ??= AppDomain.CurrentDomain.BaseDirectory;
        references ??= Array.Empty<DynamicAssembly>();

        Name = nameWithoutExtension;
        Namespace = nameWithoutExtension;
        var fileExtension = executable ? "exe" : "dll";
        FileName =
            $"{Namespace}{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{Interlocked.Increment(ref dynamicAssemblyId)}.{fileExtension}";
        DynamicName = Path.GetFileNameWithoutExtension(FileName);
        AssemblyDirectory = assemblyDirectory;

        var builder = new StringBuilder();
        builder.AppendLine("using System.Reflection;");
        builder.AppendLine($"[assembly: AssemblyVersion(\"{version}\")]");
        builder.AppendLine($"[assembly: AssemblyFileVersion(\"{version}\")]");

        builder.AppendFormat("namespace {0} {{ ", Namespace);

        var provider = new CSharpCodeProvider();
        var param = new CompilerParameters(new string[]
        {
        }, FileName)
        {
            GenerateExecutable = false,
            GenerateInMemory = false,
            OutputAssembly = FilePath = Path.Combine(AssemblyDirectory, FileName),
            TempFiles = new TempFileCollection(AssemblyDirectory, false)
        };

        foreach (var reference in references)
        {
            builder.AppendLine($"using {reference.Namespace};");
            param.ReferencedAssemblies.Add(reference.FilePath);
        }

        if (executable)
        {
            param.GenerateExecutable = true;
            builder.AppendLine("public static class Program { public static void Main(string[] args){} }");
        }

        if (content == null)
        {
            builder.AppendLine("public class Foo { public Foo() {");
            foreach (var reference in references)
            {
                builder.AppendLine($"new {reference.Namespace}.Foo();");
            }

            builder.AppendLine("} }");
        }
        else
        {
            builder.AppendLine(content);
        }

        builder.AppendLine(" }");

        var result = provider.CompileAssemblyFromSource(param, builder.ToString());
        ThrowIfCompilationWasNotSuccessful(result);
        provider.Dispose();

        if (fakeIdentity)
        {
            using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(FilePath, new ReaderParameters
                   {
                       ReadWrite = true
                   }))
            {
                assemblyDefinition.Name.Name = nameWithoutExtension;
                assemblyDefinition.MainModule.Name = nameWithoutExtension;
                assemblyDefinition.Write();
            }
        }

        Assembly = result.CompiledAssembly;
    }

    public string Namespace { get; }

    public string Name { get; }

    public string DynamicName { get; }

    public string FileName { get; }

    public string FilePath { get; }

    public Assembly Assembly { get; }

    public string AssemblyDirectory { get; set; }

    static void ThrowIfCompilationWasNotSuccessful(CompilerResults results)
    {
        if (results.Errors.HasErrors)
        {
            var errors = new StringBuilder($"Compiler Errors :{Environment.NewLine}");
            foreach (CompilerError error in results.Errors)
            {
                errors.Append($"Line {error.Line},{error.Column}\t: {error.ErrorText}{Environment.NewLine}");
            }

            throw new Exception(errors.ToString());
        }
    }

    public static implicit operator Assembly(DynamicAssembly dynamicAssembly) => dynamicAssembly.Assembly;

    static long dynamicAssemblyId;
}