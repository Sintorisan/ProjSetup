using Enums;
using Interfaces;
using Models;
using Spectre.Console;
using UI;

namespace SetupSteps;

public class ProjectInfoStep : IProjectSetupStep
{
    private const SetupStep Step = SetupStep.ProjectInfo;
    private const string Title = "Project Information";

    public void Execute(ProjectOptions options)
    {
        Redraw(options, "Basic settings for your Web API");

        options.ProjectName = PromptProjectName();
        Redraw(options);

        options.ProjectPath = PromptProjectPath(options);
        Redraw(options);

        options.ApiType = PromptApiType();
    }

    private void Redraw(ProjectOptions options, string? subtitle = null)
    {
        UiComponents.Redraw(
            options,
            Step,
            Title,
            subtitle
        );
    }

    private string PromptProjectName()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>("[bold green]> Project name[/]: ")
                .PromptStyle("green")
                .Validate(name =>
                    string.IsNullOrWhiteSpace(name)
                        ? ValidationResult.Error("[red]Project name is required[/]")
                        : ValidationResult.Success())
        );
    }

    private string PromptProjectPath(ProjectOptions options)
    {
        AnsiConsole.WriteLine();

        var basePath = Environment.CurrentDirectory;

        if (AnsiConsole.Confirm(
                $"[bold green]> Use current directory[/]?\n[grey]{basePath}[/]"))
        {
            return basePath;
        }

        return BrowseDirectories(options, basePath);
    }

    private ApiType PromptApiType()
    {
        AnsiConsole.WriteLine();

        return AnsiConsole.Prompt(
            new SelectionPrompt<ApiType>()
                .Title("[bold green]> API type[/]:")
                .HighlightStyle("cyan")
                .AddChoices(Enum.GetValues<ApiType>())
        );
    }

    private string BrowseDirectories(ProjectOptions options, string startPath)
    {
        var currentPath = startPath;

        while (true)
        {
            Redraw(options, currentPath);

            var directories = Directory.GetDirectories(currentPath)
                .Select(Path.GetFileName)
                .OrderBy(name => name)
                .ToList();

            var choices = new List<string>
            {
                "[green]✔ Select this directory[/]",
                "[grey]⬆ Parent directory[/]"
            };

            choices.AddRange(directories!);
            choices.Add("[red]✖ Cancel[/]");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]> Choose directory[/]:")
                    .HighlightStyle("cyan")
                    .AddChoices(choices)
            );

            if (choice.Contains("Select this directory"))
                return currentPath;

            if (choice.Contains("Cancel"))
                return startPath;

            if (choice.Contains("Parent"))
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
