﻿using static SimpleExec.Command;

Console.WriteLine("Writing non handler projects");

var numberOfDependencyProjects = 370;
var numberOfHandlerProjects = 17;

for (var i = 1; i < numberOfDependencyProjects; i++)
{
    var dependencyDirectory = $"../../../../Dependency{i}";
    if (Directory.Exists(dependencyDirectory))
    {
        Directory.Delete(dependencyDirectory, true);
    }
    var destinationDirectory = Directory.CreateDirectory(dependencyDirectory);
    File.Copy("../../../../EmptyTemplate/EmptyTemplate.csproj", Path.Combine(destinationDirectory.FullName, $"Dependency{i}.csproj"), true);
    var sourceFileName = Path.Combine("../../../../EmptyTemplate", "SomeTypeTemplate.cs");

    for (var j = 2; j < 360; j++)
    {
        var destFileName = Path.Combine(destinationDirectory.FullName, $"SomeType{j}.cs");
        File.Copy(sourceFileName, destFileName);
        var text = File.ReadAllText(destFileName);
        text = text.Replace("NamespaceTemplate", $"Dependency{i}");
        text = text.Replace("SomeTypeTemplate", $"SomeType{j}");
        File.WriteAllText(destFileName, text);
    }
}

Console.WriteLine("Writing handler projects");

for (var i = 1; i < numberOfHandlerProjects; i++)
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
        var text = File.ReadAllText(destFileName);
        text = text.Replace("NamespaceTemplate", $"Handler{i}");
        text = text.Replace("SomeHandlerTemplate", $"Handler{j}");
        text = text.Replace("SomeMessageTemplate", $"Message{j}");
        File.WriteAllText(destFileName, text);
        
        destFileName = Path.Combine(destinationDirectory.FullName, $"Message{j}.cs");
        File.Copy(messageFileName, destFileName);
        text = File.ReadAllText(destFileName);
        text = text.Replace("NamespaceTemplate", $"Handler{i}");
        text = text.Replace("SomeMessageTemplate", $"Message{j}");
        File.WriteAllText(destFileName, text);
    }

    // some types
    for (var j = 2; j < 345; j++)
    {
        var destFileName = Path.Combine(destinationDirectory.FullName, $"SomeType{j}.cs");
        File.Copy(sourceFileName, destFileName);
        var text = File.ReadAllText(destFileName);
        text = text.Replace("NamespaceTemplate", $"Handler{i}");
        text = text.Replace("SomeTypeTemplate", $"SomeType{j}");
        File.WriteAllText(destFileName, text);
    }
    
    // dependencies
    for (int k = 0; k < 20; k++)
    {
        // dotnet add [<PROJECT>] reference <PROJECT_PATH>.
        var random = Random.Shared.Next(1, numberOfDependencyProjects);
        Run("dotnet", $"""add {destinationProjectPath} reference ./Dependency{random}/Dependency{random}.csproj""", workingDirectory: "../../../../");
    }
}

var perfHarnessIncludes = Path.GetFullPath("../../../../AssemblyScanningPerfHarness/AssemblyScanningPerfHarness.csproj");

for (int k = 1; k < numberOfDependencyProjects; k++)
{
    Run("dotnet", $"""add {perfHarnessIncludes} reference ./Dependency{k}/Dependency{k}.csproj""", workingDirectory: "../../../../");
}

for (int k = 1; k < numberOfHandlerProjects; k++)
{
    Run("dotnet", $"""add {perfHarnessIncludes} reference ./Handler{k}/Handler{k}.csproj""", workingDirectory: "../../../../");
}
        
Run("dotnet", "new sln -n Harness --force", workingDirectory: "../../../../");
for (int k = 1; k < numberOfDependencyProjects; k++)
{
    Run("dotnet", $"sln Harness.sln add Dependency{k}/Dependency{k}.csproj", workingDirectory: "../../../../");
}
for (int k = 1; k < numberOfHandlerProjects; k++)
{
    Run("dotnet", $"sln Harness.sln add Handler{k}/Handler{k}.csproj", workingDirectory: "../../../../");
}
Run("dotnet", $"sln Harness.sln add {perfHarnessIncludes}", workingDirectory: "../../../../");
Run("dotnet", "build Harness.sln -c Release", workingDirectory: "../../../../");
