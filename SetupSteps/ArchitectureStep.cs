using Enums;
using Interfaces;
using Models;
using Spectre.Console;
using UI;

namespace SetupSteps;

public class ArchitectureStep : IProjectSetupStep
{
    public void Execute(ProjectOptions opt)
    {
        if (opt.Structure == null)
        {
            opt.Structure = CreateInitialStructure(opt);
        }

        while (true)
        {
            AnsiConsole.Clear();
            UiComponents.RenderSummary(opt);

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<ArchitectureAction>()
                    .Title("Architecture layer setup")
                    .AddChoices(Enum.GetValues<ArchitectureAction>())
            );

            if (action == ArchitectureAction.Done)
                break;

            HandleAction(action, opt);
        }
    }

    private ProjectStructure CreateInitialStructure(ProjectOptions opt)
    {
        var structure = new ProjectStructure();

        var defaultFolders = opt.ApiType == ApiType.Controllers
                ? new[] { "Controllers", "Properties" }
                : new[] { "Properties" };

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
                RemoveNode(opt.Structure);
                break;

            case ArchitectureAction.Rename:
                RenameNode(opt.Structure);
                break;

            case ArchitectureAction.Inspect:
                InspectNode(opt);
                break;
        }
    }

    private void CreateNode(ProjectOptions opt)
    {
        var layerType = AnsiConsole.Prompt(
            new SelectionPrompt<LayerType>()
                .Title("Select layer type")
                .AddChoices(Enum.GetValues<LayerType>())
        );

        var name = $"{opt.ProjectName}.{layerType}";

        if (!AnsiConsole.Confirm($"Keep layer name?\n[grey]{name}[/]"))
        {
            name = AnsiConsole.Ask<string>($"New name for {layerType} layer: ") + $".{layerType}";
        }

        if (opt.Structure.Layers.Any(l => l.Name == name))
        {
            AnsiConsole.MarkupLine("[red]A project with that name already exists[/]");
            return;
        }


        opt.Structure.Layers.Add(new Node
        {
            Name = name,
            Type = ProjectType.ClassLibrary,
            Layer = layerType
        });
    }

    private void RemoveNode(ProjectStructure structure)
    {
        var removable = structure.Layers
            .Where(l => l.Type != ProjectType.Api)
            .ToList();

        if (!removable.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No removable projects available[/]");
            return;
        }

        var node = AnsiConsole.Prompt(
            new SelectionPrompt<Node>()
                .Title("Select project to remove")
                .UseConverter(n => n.Name)
                .AddChoices(removable)
        );

        if (AnsiConsole.Confirm($"Remove [red]{node.Name}[/]?"))
        {
            structure.Layers.Remove(node);
        }
    }

    private void RenameNode(ProjectStructure structure)
    {
        var node = PromptSelectNode(structure, "Select project to rename");

        var newName = PromptFolderName(
            $"New name for {node.Name}:"
        );

        if (node.Type == ProjectType.Api && !newName.EndsWith(".API"))
        {
            newName += ".API";
        }

        node.Name = newName;
    }

    private void InspectNode(ProjectOptions opt)
    {
        var node = PromptSelectNode(opt.Structure, "Inspect project");

        while (true)
        {
            AnsiConsole.Clear();
            UiComponents.RenderSummary(opt);

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Inspecting [bold]{node.Name}[/]")
                    .AddChoices(
                        "Add folder",
                        "Rename folder",
                        "Remove folder",
                        "Back"
                    )
            );

            if (action == "Back")
                break;

            switch (action)
            {
                case "Add folder":
                    var folderName = PromptFolderName("Folder name:");
                    node.Folders.Add(folderName);
                    break;

                case "Rename folder":
                    if (!node.Folders.Any())
                        break;

                    var old = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select folder")
                            .AddChoices(node.Folders)
                    );

                    var renamed = PromptFolderName("New folder name:");
                    node.Folders[node.Folders.IndexOf(old)] = renamed;
                    break;

                case "Remove folder":
                    if (!node.Folders.Any())
                        break;

                    var folder = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select folder to remove")
                            .AddChoices(node.Folders)
                    );

                    if (AnsiConsole.Confirm($"Remove folder {folder}?"))
                        node.Folders.Remove(folder);
                    break;
            }
        }

    }

    private Node PromptSelectNode(ProjectStructure structure, string title)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<Node>()
                .Title(title)
                .UseConverter(n => n.Name)
                .AddChoices(structure.Layers)
        );
    }

    private string PromptFolderName(string title)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>(title)
                .Validate(name =>
                    string.IsNullOrWhiteSpace(name)
                        ? ValidationResult.Error("[red]Name cannot be empty[/]")
                        : ValidationResult.Success()
                )
        );
    }

}