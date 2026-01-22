using Interfaces;
using Models;
using SetupSteps;
using Spectre.Console;
using UI;

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
        UiComponents.RenderSummary(options);
        steps[i].Execute(options);

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