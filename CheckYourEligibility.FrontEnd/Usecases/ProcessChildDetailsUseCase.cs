using CheckYourEligibility.FrontEnd.Models;

namespace CheckYourEligibility.FrontEnd.UseCases;

public interface IProcessChildDetailsUseCase
{
    Task<FsmApplication> Execute(Children children, ISession session);
}

public class ProcessChildDetailsUseCase : IProcessChildDetailsUseCase
{
    public Task<FsmApplication> Execute(Children children, ISession session)
    {
        var fsmApplication = new FsmApplication
        {
            ParentFirstName = session.GetString("ParentFirstName"),
            ParentLastName = session.GetString("ParentLastName"),
            ParentDateOfBirth = session.GetString("ParentDOB"),
            ParentNass = session.GetString("ParentNASS") ?? null,
            ParentNino = session.GetString("ParentNINO") ?? null,
            Email = session.GetString("Email"),
            Children = children
        };

        return Task.FromResult(fsmApplication);
    }
}