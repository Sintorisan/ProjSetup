using Interfaces;
using Models;
using SetupSteps;
using Spectre.Console;

Console.Clear();

var options = new ProjectOptions();

var steps = new List<IProjectSetupStep>
{
    new ProjectInfoStep(),
    new ArchitectureStep(),
    new AuthStep(),
    new DatabaseStep(),
};


for (int i = 0; i < steps.Count; i++)
{
    while (true)
    {
        AnsiConsole.Clear();
        steps[i].Execute(options);
        RenderSummary();

        var nextStep = i == steps.Count - 1
            ? "Build"
            : GetStepDisplayName(steps[i + 1]);

        if (MoveOnToNext(nextStep))
        {
            break;
        }
    }
}

string GetStepDisplayName(IProjectSetupStep step)
{
    return step.GetType().Name
        .Replace("Step", string.Empty);
}

bool MoveOnToNext(string step)
{
    return AnsiConsole.Confirm(
        $"Move on to [bold]{step}[/]?"
    );
}

void RenderSummary()
{
    var summary = new Panel(BuildSummaryTable(options))
    {
        Header = new PanelHeader("Current Configuration"),
        Border = BoxBorder.Rounded
    };

    AnsiConsole.Write(summary);
}

Table BuildSummaryTable(ProjectOptions options)
{
    var table = new Table()
        .RoundedBorder()
        .AddColumn("Option")
        .AddColumn("Value");

    table.AddRow("Project Name", options.ProjectName ?? "[grey]Not set[/]");
    table.AddRow("Project Path", options.ProjectPath ?? "[grey]Not set[/]");
    table.AddRow("Api Type", options.ApiType?.ToString() ?? "[grey]Not set[/]");
    // table.AddRow("Architecture", options.Architecture?.ToString() ?? "[grey]Not set[/]");
    // table.AddRow("Database", options.Database?.ToString() ?? "[grey]Not set[/]");
    // table.AddRow("Auth", options.Auth?.ToString() ?? "[grey]Not set[/]");

    return table;
}