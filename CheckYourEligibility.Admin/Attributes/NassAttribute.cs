using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using static CheckYourEligibility.Admin.Models.ParentGuardian;

namespace CheckYourEligibility.Admin.Attributes
{
    public class NassAttribute : ValidationAttribute
    {
        private static readonly string NassPattern = @"^[0-9]{2}(0[1-9]|1[0-2])[0-9]{5,6}$";
        private static readonly Regex regex = new Regex(NassPattern);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (ParentGuardian)validationContext.ObjectInstance;

            //If NIN is selected stop validating ASR option
            if (model.NinAsrSelection == NinAsrSelect.NinSelected)
            {
                return ValidationResult.Success;
            }

            //Neither option selected
            //Handled in NINO. Allow success result here to avoid double validation message
            if (model.NinAsrSelection == NinAsrSelect.None)
            {
                return new ValidationResult("Please select one option");
            }

            //ASR Selected but not provided
            if (model.NinAsrSelection == NinAsrSelect.AsrnSelected && value == null)
            {
                return new ValidationResult("Asylum support reference number is required");
            }

            //Asr selected and completed - validate against regex
            if (model.NinAsrSelection == NinAsrSelect.AsrnSelected)
            {
                if (!regex.IsMatch(value.ToString()))
                {
                    return new ValidationResult("Nass field contains an invalid character");
                }
            }

            return ValidationResult.Success;
        }
    }
}
