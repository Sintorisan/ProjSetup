using Enums;
using Interfaces;
using Microsoft.VisualBasic;
using Models;
using Spectre.Console;

namespace SetupSteps;

public class ProjectInfoStep : IProjectSetupStep
{
    public void Execute(ProjectOptions options)
    {
        AnsiConsole.Clear();

        options.ProjectName = AnsiConsole.Ask<string>("Project name:");
        options.ProjectPath = AskForProjectPath();
        options.ApiType = AskForApiType();
    }
    private ApiType? AskForApiType()
    {
        AnsiConsole.Clear();
        var type = AnsiConsole.Prompt(
            new SelectionPrompt<ApiType>()
            .Title("[bold]Choose API type: [/] ")
            .AddChoices(Enum.GetValues<ApiType>()));

        return type;
    }
    private string AskForProjectPath()
    {
        var basePath = Environment.CurrentDirectory;

        if (AnsiConsole.Confirm(
                $"Add project to current directory?\n[grey]{basePath}[/]"))
        {
            return basePath;
        }

        return BrowseDirectories(basePath);
    }
    private string BrowseDirectories(string startPath)
    {
        var currentPath = startPath;

        while (true)
        {
            var directories = Directory.GetDirectories(currentPath)
                .Select(Path.GetFileName)
                .OrderBy(name => name)
                .ToList();

            var choices = new List<string>
            {
                "[[Select this directory]]",
                ".."
            };

            choices.AddRange(directories!);
            choices.Add("[[Cancel]]");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Current directory:\n[grey]{currentPath}[/]")
                    .AddChoices(choices)
            );

            if (choice == "[[Select this directory]]")
            {
                return currentPath;
            }

            if (choice == "[[Cancel]]")
            {
                return startPath;
            }

            if (choice == "..")
            {
                var parent = Directory.GetParent(currentPath);
                if (parent != null)
                    currentPath = parent.FullName;

                continue;
            }

            currentPath = Path.Combine(currentPath, choice);
        }
    }
}