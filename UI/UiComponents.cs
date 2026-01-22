using Enums;
using Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace UI;

public static class UiComponents
{
    public static void ShowLandingPage()
    {
        AnsiConsole.Clear();

        var content = new Rows(
            new Markup("[bold white]Web API Scaffolder[/]"),
            new Markup("[grey]Create a clean ASP.NET Web API project[/]"),
            new Markup("[grey] [/]"),
            new Markup("[bold]Steps[/]"),
            new Markup("[grey]1. Project Info[/]"),
            new Markup("[grey]2. Architecture[/]"),
            new Markup("[grey]3. Database[/]"),
            new Markup("[grey]4. Endpoints[/]"),
            new Markup("[grey] [/]"),
            new Markup("[bold green]Press Enter to start[/]")
        );

        var panel = new Panel(content)
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(2, 1),
            Expand = true
        };

        AnsiConsole.Write(panel);
        Console.ReadLine();
    }

    public static void Redraw(
        ProjectOptions options,
        SetupStep currentStep,
        string title,
        string? subtitle = null)
    {
        AnsiConsole.Clear();

        var headerContent = new Rows(
            BuildBreadcrumb(currentStep),
            new Markup($"[bold white]{title}[/]"),
            new Markup(
                string.IsNullOrWhiteSpace(subtitle)
                    ? "[grey] [/]"
                    : $"[grey]{subtitle}[/]"
            )
        );

        var header = new Panel(headerContent)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Blue),
            Padding = new Padding(2, 1),
            Expand = true
        };

        AnsiConsole.Write(header);
        AnsiConsole.WriteLine();

        RenderSummary(options);

        AnsiConsole.WriteLine();
    }

    private static IRenderable BuildBreadcrumb(SetupStep currentStep)
    {
        var steps = Enum.GetValues<SetupStep>();

        var items = steps.Select(step =>
            step == currentStep
                ? $"[bold cyan]{FormatStep(step)}[/]"
                : $"[grey]{FormatStep(step)}[/]"
        );

        return new Markup(string.Join(" [grey]>[/] ", items));
    }

    private static string FormatStep(SetupStep step)
    {
        return step switch
        {
            SetupStep.ProjectInfo => "Project Info",
            SetupStep.Architecture => "Architecture",
            SetupStep.Database => "Database",
            SetupStep.Endpoint => "Endpoint",
            SetupStep.Build => "Build",
            _ => step.ToString()
        };
    }


    private static void RenderSummary(ProjectOptions options)
    {
        var columns = new Columns(
            BuildSummaryPanel(options),
            BuildStructurePanel(options.Structure)
        )
        {
            Padding = new Padding(2, 0)
        };

        AnsiConsole.Write(columns);
    }

    private static Panel BuildSummaryPanel(ProjectOptions options)
    {
        return new Panel(BuildSummaryTable(options))
        {
            Header = new PanelHeader("[bold cyan]Configuration[/]"),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1)
        };
    }

    private static Table BuildSummaryTable(ProjectOptions options)
    {
        var table = new Table { Border = TableBorder.Rounded };

        table.AddColumn(new TableColumn("[grey]Option[/]").Centered());
        table.AddColumn(new TableColumn("[grey]Value[/]").Centered());

        table.AddRow("Project Name",
            options.ProjectName ?? "[yellow]Not set[/]");

        table.AddRow("API Type",
            options.ApiType != null
                ? $"[cyan]{options.ApiType}[/]"
                : "[yellow]Not set[/]");

        table.AddRow("Database",
            options.DbOptions.DatabaseType != DatabaseType.None
                ? "[green]✔ Enabled[/]"
                : "[grey]✖ Disabled[/]");

        table.AddRow("EF Core",
            options.DbOptions.HasEfCore
                ? "[green]✔ Yes[/]"
                : "[grey]✖ No[/]");

        table.AddRow("Authentication",
            options.DbOptions.UseIdentity
                ? "[green]✔ Identity[/]"
                : "[grey]✖ None[/]");

        return table;
    }

    private static Panel BuildStructurePanel(ProjectStructure? structure)
    {
        var tree = new Tree("[bold cyan]Solution Structure[/]");

        if (structure == null)
        {
            tree.AddNode("[grey]No structure defined[/]");
        }
        else
        {
            foreach (var layer in structure.Layers)
            {
                var projectLabel =
                    layer.Type == ProjectType.Api
                        ? $"[bold yellow]🚀 {layer.Name} (API)[/]"
                        : $"[green]🧱 {layer.Name} ({layer.Layer})[/]";

                var projectNode = tree.AddNode(projectLabel);

                if (!layer.Folders.Any())
                {
                    projectNode.AddNode("[grey]No folders[/]");
                    continue;
                }

                foreach (var folder in layer.Folders)
                {
                    var folderNode = projectNode.AddNode(
                        $"[blue]📁 {folder.Name}[/]"
                    );

                    foreach (var file in folder.Files)
                    {
                        folderNode.AddNode($"[grey]📄 {file}[/]");
                    }
                }
            }
        }

        return new Panel(tree)
        {
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1),
        };
    }
}
