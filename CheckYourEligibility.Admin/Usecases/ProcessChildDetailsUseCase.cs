using CheckYourEligibility.Admin.Models;
using Microsoft.AspNetCore.Http;

namespace CheckYourEligibility.Admin.UseCases
{
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
                ParentEmail = session.GetString("ParentEmail"),
                Children = children
            };

            return Task.FromResult(fsmApplication);
        }
    }
}