using Enums;
using Models;
using Spectre.Console;

namespace UI;

public static class UiComponents
{
    public static void RenderSummary(ProjectOptions options)
    {
        var columns = new Columns(
            BuildSummaryPanel(options),
            BuildStructurePanel(options.Structure)
        )
        {
            Expand = false,   // <-- IMPORTANT
            Padding = new Padding(2, 0)
        };

        AnsiConsole.Write(columns);
    }

    private static Panel BuildSummaryPanel(ProjectOptions options)
    {
        return new Panel(BuildSummaryTable(options))
        {
            Header = new PanelHeader("Configuration"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1)
        };
    }

    private static Table BuildSummaryTable(ProjectOptions options)
    {
        var table = new Table()
            .RoundedBorder()
            .AddColumn("Option")
            .AddColumn("Value");

        table.AddRow("Project Name", options.ProjectName ?? "[grey]Not set[/]");
        table.AddRow("Project Path", options.ProjectPath ?? "[grey]Not set[/]");
        table.AddRow("API Type", options.ApiType?.ToString() ?? "[grey]Not set[/]");

        return table;
    }

    private static Panel BuildStructurePanel(ProjectStructure? structure)
    {
        var tree = new Tree("[bold]Solution[/]");
        if (structure == null)
        {
            return new Panel(tree)
            {
                Header = new PanelHeader("Project Structure"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1),
                Width = 60
            };
        }

        foreach (var layer in structure.Layers)
        {
            var label = layer.Type == ProjectType.Api
                ? $"[bold yellow]{layer.Name}[/] [grey](API)[/]"
                : $"[green]{layer.Name}[/] [grey]({layer.Layer})[/]";

            var projectNode = tree.AddNode(label);

            if (layer.Folders.Any())
            {
                foreach (var folder in layer.Folders)
                {
                    projectNode.AddNode($"[blue]{folder}[/]");
                }
            }
            else
            {
                projectNode.AddNode("[grey]No folders[/]");
            }
        }

        return new Panel(tree)
        {
            Header = new PanelHeader("Project Structure"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1),
            Width = 60
        };
    }
}
