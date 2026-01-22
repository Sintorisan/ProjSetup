using Enums;
using Interfaces;
using Models;
using Spectre.Console;
using UI;

namespace SetupSteps;

public class ArchitectureStep : IProjectSetupStep
{
    private const SetupStep Step = SetupStep.Architecture;
    private const string Title = "Architecture Setup";

    public void Execute(ProjectOptions opt)
    {
        if (opt.Structure == null)
            opt.Structure = CreateInitialStructure(opt);

        while (true)
        {
            Redraw(opt, "Manage solution projects and folders");

            var action = PromptArchitectureAction();

            if (action == ArchitectureAction.Done)
                break;

            HandleAction(action, opt);
        }
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

    private ArchitectureAction PromptArchitectureAction()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<ArchitectureAction>()
                .Title("[bold green]> Choose action[/]:")
                .HighlightStyle("cyan")
                .AddChoices(Enum.GetValues<ArchitectureAction>())
        );
    }

    private ProjectStructure CreateInitialStructure(ProjectOptions opt)
    {
        var structure = new ProjectStructure();

        var defaultFolders =
            opt.ApiType == ApiType.Controllers
                ? new[]
                {
                    new Folder { Name = "Controllers" },
                    new Folder { Name = "Properties" }
                }
                : new[]
                {
                    new Folder { Name = "Properties" }
                };

        structure.Layers.Add(new Node
        {
            Name = $"{opt.ProjectName}.API",
            Type = ProjectType.Api,
            Folders = defaultFolders.ToList()
        });

        return structure;
    }

    private void HandleAction(ArchitectureAction action, ProjectOptions opt)
    {
        switch (action)
        {
            case ArchitectureAction.Add:
                CreateNode(opt);
                break;

            case ArchitectureAction.Remove:
                RemoveNode(opt);
                break;

            case ArchitectureAction.Rename:
                RenameNode(opt);
                break;

            case ArchitectureAction.Inspect:
                InspectNode(opt);
                break;
        }
    }

    private void CreateNode(ProjectOptions opt)
    {
        Redraw(opt, "Add project");

        var layerType = AnsiConsole.Prompt(
            new SelectionPrompt<LayerType>()
                .Title("[bold green]> Layer type[/]:")
                .HighlightStyle("cyan")
                .AddChoices(Enum.GetValues<LayerType>())
        );

        var defaultName = $"{opt.ProjectName}.{layerType}";
        var name = defaultName;

        if (!AnsiConsole.Confirm(
                $"[bold green]> Keep default name[/]?\n[grey]{defaultName}[/]"))
        {
            name = PromptName("Layer name") + $".{layerType}";
        }

        if (opt.Structure!.Layers.Any(l => l.Name == name))
        {
            AnsiConsole.MarkupLine("[red]Project already exists[/]");
            Console.ReadKey();
            return;
        }

        opt.Structure.Layers.Add(new Node
        {
            Name = name,
            Type = ProjectType.ClassLibrary,
            Layer = layerType
        });
    }

    private void RemoveNode(ProjectOptions opt)
    {
        Redraw(opt, "Remove project");

        var removable = opt.Structure!.Layers
            .Where(l => l.Type != ProjectType.Api)
            .ToList();

        if (!removable.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No removable projects[/]");
            Console.ReadKey();
            return;
        }

        var node = PromptSelectNode(removable, "Select project");

        if (AnsiConsole.Confirm($"[red]Remove[/] {node.Name}?"))
            opt.Structure.Layers.Remove(node);
    }

    private void RenameNode(ProjectOptions opt)
    {
        Redraw(opt, "Rename project");

        var node = PromptSelectNode(
            opt.Structure!.Layers,
            "Select project"
        );

        var newName = PromptName($"New name for {node.Name}");

        if (node.Type == ProjectType.Api && !newName.EndsWith(".API"))
            newName += ".API";

        node.Name = newName;
    }

    private void InspectNode(ProjectOptions opt)
    {
        var node = PromptSelectNode(
            opt.Structure!.Layers,
            "Inspect project"
        );

        while (true)
        {
            Redraw(opt, $"Inspecting {node.Name}");

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]> Folder action[/]:")
                    .HighlightStyle("cyan")
                    .AddChoices(
                        "Add folder",
                        "Rename folder",
                        "Remove folder",
                        "Back"
                    )
            );

            if (action == "Back")
                break;

            HandleFolderAction(action, node);
        }
    }

    private void HandleFolderAction(string action, Node node)
    {
        switch (action)
        {
            case "Add folder":
                var name = PromptName("Folder name");

                if (node.Folders.Any(f => f.Name == name))
                {
                    AnsiConsole.MarkupLine("[red]Folder already exists[/]");
                    Console.ReadKey();
                    return;
                }

                node.Folders.Add(new Folder { Name = name });
                break;

            case "Rename folder":
                if (!node.Folders.Any())
                    return;

                var folder = PromptSelectFolder(node, "Select folder");
                folder.Name = PromptName("New folder name");
                break;

            case "Remove folder":
                if (!node.Folders.Any())
                    return;

                var remove = PromptSelectFolder(node, "Select folder");

                if (AnsiConsole.Confirm($"Remove {remove.Name}?"))
                    node.Folders.Remove(remove);
                break;
        }
    }

    private Node PromptSelectNode(
        IEnumerable<Node> nodes,
        string title)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<Node>()
                .Title($"[bold green]> {title}[/]:")
                .UseConverter(n => n.Name)
                .HighlightStyle("cyan")
                .AddChoices(nodes)
        );
    }

    private Folder PromptSelectFolder(Node node, string title)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<Folder>()
                .Title($"[bold green]> {title}[/]:")
                .UseConverter(f => f.Name)
                .HighlightStyle("cyan")
                .AddChoices(node.Folders)
        );
    }

    private string PromptName(string label)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[bold green]> {label}[/]: ")
                .PromptStyle("green")
                .Validate(name =>
                    string.IsNullOrWhiteSpace(name)
                        ? ValidationResult.Error("[red]Name cannot be empty[/]")
                        : ValidationResult.Success())
        );
    }
}
