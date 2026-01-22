using Enums;
using Interfaces;
using Models;
using Spectre.Console;
using UI;

namespace SetupSteps;

public class DatabaseStep : IProjectSetupStep
{
    private const SetupStep Step = SetupStep.Database;
    private const string Title = "Database Setup";

    public void Execute(ProjectOptions opt)
    {
        Redraw(opt, "Configure database and persistence");

        if (!AnsiConsole.Confirm("[bold green]> Add a database[/]?"))
            return;

        SelectDatabaseType(opt);
        Redraw(opt);

        ConfigureEfCore(opt);
        Redraw(opt);

        ConfigureAuthentication(opt);
        Redraw(opt);

        ConfigureEntities(opt);
        Redraw(opt);
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

    private void SelectDatabaseType(ProjectOptions opt)
    {
        var dbType = AnsiConsole.Prompt(
            new SelectionPrompt<DatabaseType>()
                .Title("[bold green]> Database provider[/]:")
                .HighlightStyle("cyan")
                .AddChoices(
                    Enum.GetValues<DatabaseType>()
                        .Where(d => d != DatabaseType.None)
                )
        );

        opt.DbOptions.DatabaseType = dbType;
    }

    private void ConfigureEfCore(ProjectOptions opt)
    {
        opt.DbOptions.HasEfCore = AnsiConsole.Confirm(
            "[bold green]> Add EF Core[/]?"
        );
    }

    private void ConfigureAuthentication(ProjectOptions opt)
    {
        if (!opt.DbOptions.HasEfCore)
        {
            AnsiConsole.MarkupLine(
                "[yellow]Authentication requires EF Core[/]"
            );
            Console.ReadKey();
            return;
        }

        AnsiConsole.MarkupLine(
            "[grey]ASP.NET Identity adds users, roles, and required tables[/]"
        );

        opt.DbOptions.UseIdentity = AnsiConsole.Confirm(
            "[bold green]> Enable authentication (ASP.NET Identity)[/]?"
        );
    }

    private void ConfigureEntities(ProjectOptions opt)
    {
        if (!AnsiConsole.Confirm("[bold green]> Add entities[/]?"))
            return;

        var layer = SelectLayer(opt.Structure!.Layers);
        Redraw(opt, $"Entities in {layer.Name}");

        var folder = SelectFolder(layer);
        if (folder == null)
            return;

        while (true)
        {
            var fileName = PromptEntityName();

            if (folder.Files.Any(f => f == fileName))
            {
                AnsiConsole.MarkupLine(
                    "[red]Entity already exists in this folder[/]"
                );
                Console.ReadKey();
                continue;
            }

            folder.Files.Add(fileName);
            Redraw(opt, $"Entity added to {folder.Name}");

            if (!AnsiConsole.Confirm("[bold green]> Add another entity[/]?"))
                break;
        }
    }

    private Node SelectLayer(List<Node> layers)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<Node>()
                .Title("[bold green]> Target project[/]:")
                .UseConverter(n => n.Name)
                .HighlightStyle("cyan")
                .AddChoices(layers)
        );
    }

    private Folder? SelectFolder(Node layer)
    {
        var choices = new List<string>();

        choices.AddRange(layer.Folders.Select(f => f.Name));
        choices.Add("[green]+ New folder[/]");
        choices.Add("[grey]Done[/]");

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold green]> Target folder[/]:")
                .HighlightStyle("cyan")
                .AddChoices(choices)
        );

        if (selection.Contains("Done"))
            return null;

        if (selection.Contains("New folder"))
            return CreateNewFolder(layer);

        return layer.Folders.First(f => f.Name == selection);
    }

    private Folder CreateNewFolder(Node layer)
    {
        var folderName = PromptName("Folder name");

        var existing = layer.Folders.FirstOrDefault(f => f.Name == folderName);
        if (existing != null)
        {
            AnsiConsole.MarkupLine(
                "[yellow]Folder already exists. Using existing folder[/]"
            );
            Console.ReadKey();
            return existing;
        }

        var folder = new Folder { Name = folderName };
        layer.Folders.Add(folder);
        return folder;
    }

    private string PromptEntityName()
    {
        return PromptName("Entity name");
    }

    private string PromptName(string label)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[bold green]> {label}[/]: ")
                .PromptStyle("green")
                .Validate(name =>
                    string.IsNullOrWhiteSpace(name)
                        ? ValidationResult.Error("[red]Name cannot be empty[/]")
                        : ValidationResult.Success()
                )
        );
    }
}
