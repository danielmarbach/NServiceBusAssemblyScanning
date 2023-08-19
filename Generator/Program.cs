using static SimpleExec.Command;

Console.WriteLine("Writing non handler projects");

var numberOfDependencyProjects = 370;
var numberOfHandlerProjects = 17;

await Parallel.ForEachAsync(Enumerable.Range(1, numberOfDependencyProjects), async (i, cancellationToken) =>
{
    var dependencyDirectory = $"../../../../Dependency{i}";
    if (Directory.Exists(dependencyDirectory))
    {
        Directory.Delete(dependencyDirectory, true);
    }

    var destinationDirectory = Directory.CreateDirectory(dependencyDirectory);
    File.Copy("../../../../EmptyTemplate/EmptyTemplate.csproj",
        Path.Combine(destinationDirectory.FullName, $"Dependency{i}.csproj"), true);
    var sourceFileName = Path.Combine("../../../../EmptyTemplate", "SomeTypeTemplate.cs");

    for (var j = 2; j < 360; j++)
    {
        var destFileName = Path.Combine(destinationDirectory.FullName, $"SomeType{j}.cs");
        File.Copy(sourceFileName, destFileName);
        var text = await File.ReadAllTextAsync(destFileName, cancellationToken);
        text = text.Replace("NamespaceTemplate", $"Dependency{i}");
        text = text.Replace("SomeTypeTemplate", $"SomeType{j}");
        await File.WriteAllTextAsync(destFileName, text, cancellationToken);
    }
});

Console.WriteLine("Writing handler projects");

await Parallel.ForEachAsync(Enumerable.Range(1, numberOfHandlerProjects), async (i, cancellationToken) =>
{
    var handlerDirectory = $"../../../../Handler{i}";
    if (Directory.Exists(handlerDirectory))
    {
        Directory.Delete(handlerDirectory, true);
    }
    var destinationDirectory = Directory.CreateDirectory(handlerDirectory);
    var destinationProjectPath = Path.Combine(destinationDirectory.FullName, $"Handler{i}.csproj");
    File.Copy("../../../../HandlerTemplate/HandlerTemplate.csproj", destinationProjectPath, true);
    var sourceFileName = Path.Combine("../../../../HandlerTemplate", "SomeTypeTemplate.cs");
    var handlerFileName = Path.Combine("../../../../HandlerTemplate", "SomeHandlerTemplate.cs");
    var messageFileName = Path.Combine("../../../../HandlerTemplate", "SomeMessageTemplate.cs");

    // handlers
    for (var j = 2; j < 15; j++)
    {
        var destFileName = Path.Combine(destinationDirectory.FullName, $"Handler{j}.cs");
        File.Copy(handlerFileName, destFileName);
        var text = await File.ReadAllTextAsync(destFileName, cancellationToken);
        text = text.Replace("NamespaceTemplate", $"Handler{i}");
        text = text.Replace("SomeHandlerTemplate", $"Handler{j}");
        text = text.Replace("SomeMessageTemplate", $"Message{j}");
        await File.WriteAllTextAsync(destFileName, text, cancellationToken);
        
        destFileName = Path.Combine(destinationDirectory.FullName, $"Message{j}.cs");
        File.Copy(messageFileName, destFileName);
        text = File.ReadAllText(destFileName);
        text = text.Replace("NamespaceTemplate", $"Handler{i}");
        text = text.Replace("SomeMessageTemplate", $"Message{j}");
        await File.WriteAllTextAsync(destFileName, text, cancellationToken);
    }

    // some types
    for (var j = 2; j < 345; j++)
    {
        var destFileName = Path.Combine(destinationDirectory.FullName, $"SomeType{j}.cs");
        File.Copy(sourceFileName, destFileName);
        var text = await File.ReadAllTextAsync(destFileName, cancellationToken);;
        text = text.Replace("NamespaceTemplate", $"Handler{i}");
        text = text.Replace("SomeTypeTemplate", $"SomeType{j}");
        await File.WriteAllTextAsync(destFileName, text, cancellationToken);
    }
    
    // dependencies
    var references = new string[20];
    for (int k = 0; k < references.Length; k++)
    {
        // dotnet add [<PROJECT>] reference <PROJECT_PATH>.
        var random = Random.Shared.Next(1, numberOfDependencyProjects);
        references[k] = $"./Dependency{random}/Dependency{random}.csproj";
    }
    await RunAsync("dotnet", $"""add {destinationProjectPath} reference {string.Join(" ", references)}""", workingDirectory: "../../../../");
});

var perfHarnessIncludes = Path.GetFullPath("../../../../AssemblyScanningPerfHarness/AssemblyScanningPerfHarness.csproj");

var projects = new string[numberOfDependencyProjects + numberOfHandlerProjects - 2];

for (int k = 1; k < numberOfDependencyProjects; k++)
{
    projects[k] = $"./Dependency{k}/Dependency{k}.csproj";
}

for (int k = 1; k < numberOfHandlerProjects; k++)
{
    projects[k] = $"./Handler{k}/Handler{k}.csproj";
}

await RunAsync("dotnet", $"""add {perfHarnessIncludes} reference {string.Join(" ", projects)}""", workingDirectory: "../../../../");
await RunAsync("dotnet", "new sln -n Harness --force", workingDirectory: "../../../../");
await RunAsync("dotnet", $"sln Harness.sln add {string.Join(" ", projects)}", workingDirectory: "../../../../");
await RunAsync("dotnet", $"sln Harness.sln add {perfHarnessIncludes}", workingDirectory: "../../../../");
await RunAsync("dotnet", "build Harness.sln -c Release", workingDirectory: "../../../../");
