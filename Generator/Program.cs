// See https://aka.ms/new-console-template for more information

using System.Text;
using static SimpleExec.Command;

Console.WriteLine("Writing non handler projects");

var numberOfDependencyProjects = 2;
var numberOfHandlerProjects = 2;

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

    for (var j = 2; j < 3; j++)
    {
        var destFileName = Path.Combine(destinationDirectory.FullName, $"SomeType{j}.cs");
        File.Copy(sourceFileName, destFileName);
        var text = File.ReadAllText(destFileName);
        text = text.Replace("NamespaceTemplate", $"Handler{i}");
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
    File.Copy("../../../../HandlerTemplate/HandlerTemplate.csproj", Path.Combine(destinationDirectory.FullName, $"Handler{i}.csproj"), true);
    var sourceFileName = Path.Combine("../../../../HandlerTemplate", "SomeTypeTemplate.cs");
    var handlerFileName = Path.Combine("../../../../HandlerTemplate", "SomeHandlerTemplate.cs");
    var messageFileName = Path.Combine("../../../../HandlerTemplate", "SomeMessageTemplate.cs");

    // handlers
    for (var j = 2; j < 3; j++)
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
    for (var j = 2; j < 3; j++)
    {
        var destFileName = Path.Combine(destinationDirectory.FullName, $"SomeType{j}.cs");
        File.Copy(sourceFileName, destFileName);
        var text = File.ReadAllText(destFileName);
        text = text.Replace("NamespaceTemplate", $"Handler{i}");
        text = text.Replace("SomeTypeTemplate", $"SomeType{j}");
        File.WriteAllText(destFileName, text);
    }
    
    // dependencies
    for (var j = 2; j < 3; j++)
    {
        var destFileName = Path.Combine(destinationDirectory.FullName, "Includes.targets");
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("<Project>");
        stringBuilder.AppendLine("    <ItemGroup>");

        for (int k = 0; k < 10; k++)
        {
            var random = Random.Shared.Next(1, numberOfDependencyProjects);
            stringBuilder.AppendLine(
                $"""        <ProjectReference Include="..\Dependency{random}\Dependency{random}.csproj" />""");
        }
        
        stringBuilder.AppendLine("    </ItemGroup>");
        stringBuilder.AppendLine("</Project>");
        File.WriteAllText(destFileName, stringBuilder.ToString());
    }
}

var perfHarnessIncludes = Path.Combine("../../../../AssemblyScanningPerfHarness", "Includes.targets");
var perfHarnessIncludesBuilder = new StringBuilder();
perfHarnessIncludesBuilder.AppendLine("<Project>");
perfHarnessIncludesBuilder.AppendLine("    <ItemGroup>");

for (int k = 1; k < numberOfDependencyProjects; k++)
{
    perfHarnessIncludesBuilder.AppendLine(
        $"""        <ProjectReference Include="..\Dependency{k}\Dependency{k}.csproj" />""");
}

for (int k = 1; k < numberOfHandlerProjects; k++)
{
    perfHarnessIncludesBuilder.AppendLine(
        $"""        <ProjectReference Include="..\Handler{k}\Handler{k}.csproj" />""");
}
        
perfHarnessIncludesBuilder.AppendLine("    </ItemGroup>");
perfHarnessIncludesBuilder.AppendLine("</Project>");
File.WriteAllText(perfHarnessIncludes, perfHarnessIncludesBuilder.ToString());

Run("dotnet", "new sln -n Temp --force", workingDirectory: "../../../../");
for (int k = 1; k < numberOfDependencyProjects; k++)
{
    Run("dotnet", $"sln Temp.sln add Dependency{k}/Dependency{k}.csproj", workingDirectory: "../../../../");
}
for (int k = 1; k < numberOfHandlerProjects; k++)
{
    Run("dotnet", $"sln Temp.sln add Handler{k}/Handler{k}.csproj", workingDirectory: "../../../../");
}
Run("dotnet", "build Temp.sln -c Release", workingDirectory: "../../../../");
