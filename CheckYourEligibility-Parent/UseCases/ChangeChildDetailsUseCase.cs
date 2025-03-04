﻿using CheckYourEligibility_FrontEnd.Models;
using Newtonsoft.Json;

namespace CheckYourEligibility_FrontEnd.UseCases
{
    public interface IChangeChildDetailsUseCase
    {
        Children Execute(string fsmApplicationJson);
    }
    
    [Serializable]
    public class NoChildException : Exception
    {
        
        public NoChildException(string message) : base (message)
        {
        }
    }
    
    [Serializable]
    public class JSONException : Exception
    {
        
        public JSONException(string message) : base (message)
        {
        }
    }

    public class ChangeChildDetailsUseCase : IChangeChildDetailsUseCase
    {
        private readonly ILogger<ChangeChildDetailsUseCase> _logger;

        public ChangeChildDetailsUseCase(ILogger<ChangeChildDetailsUseCase> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Children Execute(string fsmApplicationJson)
        {
            if (string.IsNullOrEmpty(fsmApplicationJson))
            {
                throw new NoChildException("FSM Application JSON is null or empty");
            }

            var application = JsonConvert.DeserializeObject<FsmApplication>(fsmApplicationJson);
            if (application?.Children == null)
            {
                throw new JSONException("Failed to deserialize FSM Application or Children is null");
            }

            return application.Children;
        }
    }
}