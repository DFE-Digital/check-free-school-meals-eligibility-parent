using CheckYourEligibility.Admin.Models;

namespace CheckYourEligibility.Admin.UseCases
{
    public interface IRemoveChildUseCase
    {
        Task<Children> Execute(Children request, int index);
    }
    
    [Serializable]
    public class RemoveChildValidationException : Exception
    {
        
        public RemoveChildValidationException(string message) : base (message)
        {
        }
    }

    public class RemoveChildUseCase : IRemoveChildUseCase
    {
        public Task<Children> Execute(Children request, int index)
        {
            if (request?.ChildList == null)
            {
                throw new RemoveChildValidationException("Invalid request - no children list available");
            }

            if (index < 0 || index >= request.ChildList.Count)
            {
                throw new RemoveChildValidationException("Invalid child index");
            }

            var child = request.ChildList[index];
            request.ChildList.Remove(child);


            return Task.FromResult(request);
        }
    }
}