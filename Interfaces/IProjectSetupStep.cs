using Models;

namespace Interfaces;

public interface IProjectSetupStep
{
    void Execute(ProjectOptions opt);
}