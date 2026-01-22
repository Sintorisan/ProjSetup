using Interfaces;
using Models;
using SetupSteps;
using Spectre.Console;
using UI;

var options = new ProjectOptions();

var steps = new List<IProjectSetupStep>
{
    new ProjectInfoStep(),
    new ArchitectureStep(),
    new DatabaseStep(),
    new EndpointStep(),
};

UiComponents.ShowLandingPage();

foreach (var step in steps)
{
    step.Execute(options);

    if (!AnsiConsole.Confirm("[bold green]> Continue to next step[/]?"))
        break;
}

static bool ConfirmContinue(IProjectSetupStep currentStep)
{
    return AnsiConsole.Confirm(
        "[bold green]> Continue to next step[/]?"
    );
}
