using System.Reflection;
using AssemblyScanningPerfHarness;
using NServiceBus.Hosting.Helpers;

var test = new DynamicAssembly2();
var busAssembly = new DynamicAssembly("Fake.NServiceBus.Core");
var assemblyC = new DynamicAssembly("C");
var assemblyB = new DynamicAssembly("B", references: new[]
{
    busAssembly
});
var assemblyA = new DynamicAssembly("A", references: new[]
{
    assemblyB,
    assemblyC
});

var scanner = CreateDefaultAssemblyScanner(busAssembly);

Console.WriteLine("Attach the profiler and hit <enter>.");
Console.ReadLine();

var result = scanner.GetScannableAssemblies();

Console.WriteLine("To close, hit <enter>.");
Console.ReadLine();

static AssemblyScanner CreateDefaultAssemblyScanner(DynamicAssembly coreAssembly)
{
    var defaultAssemblyScanner = new AssemblyScanner(coreAssembly.AssemblyDirectory)
    {
        ScanAppDomainAssemblies = true,
        ScanFileSystemAssemblies = true,
        ThrowExceptions = true
    };
    defaultAssemblyScanner.GetType().GetProperty("CoreAssemblyName", BindingFlags.Instance | BindingFlags.NonPublic)
        .SetValue(defaultAssemblyScanner, coreAssembly.DynamicName);
    return defaultAssemblyScanner;
}