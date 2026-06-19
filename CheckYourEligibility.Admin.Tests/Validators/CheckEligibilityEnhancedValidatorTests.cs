using CheckYourEligibility.Admin.Boundary.Requests;
using CheckYourEligibility.Admin.Domain.Constants.ErrorMessages;
using CheckYourEligibility.Admin.Domain.DfeSignIn;
using CheckYourEligibility.Admin.Domain.Validation;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckYourEligibility.Admin.Tests.Validators
{
    [TestFixture]
    public class CheckEligibilityEnhancedValidatorTests
    {
        private IValidator<CheckEligibilityRequestDataBase> _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new CheckEligibilityRequestDataValidator();
        }
        
        [TestCase("abc")]
        [TestCase("0")]
        [TestCase("-1")]
        [TestCase("")]
        public async Task Validate_InvalidUrnFormat_ReturnsError(string urn)
        {
            var model = new CheckEligibilityRequestData_Enhanced
            {
                ChildSchoolUrn = urn
            };

            var context = CreateContext(model, new HashSet<int> { 123456 });

            var result = await _validator.ValidateAsync(context);

            Assert.That(result.Errors.Any(e => e.ErrorMessage == ValidationMessages.ChildSchoolUrn), Is.True);
        }
        [Test]
        public async Task Validate_UrnNotInList_ForLocalAuthority_ReturnsLAMessage()
        {
            var model = new CheckEligibilityRequestData_Enhanced
            {
                ChildSchoolUrn = "999999"
            };

            var context = CreateContext(
                model,
                validUrns: new HashSet<int> { 123456 },
                orgType: OrganisationCategory.LocalAuthority);

            var result = await _validator.ValidateAsync(context);

            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage == ValidationMessages.InvalidSchoolUrnForLA), Is.True);
        }
        [Test]
        public async Task Validate_UrnNotInList_ForMAT_ReturnsMATMessage()
        {
            var model = new CheckEligibilityRequestData_Enhanced
            {
                NationalInsuranceNumber = "AA123456C",
                LastName = "Test",
                FirstName = "Test",
                DateOfBirth = "1985-04-23",
                Sequence = 1,

                ChildFirstName = "Emily",
                ChildLastName = "Test",
                ChildDateOfBirth = "2015-09-10",
                ChildSchoolUrn = "999999"
            };

            var context = CreateContext(
                model,
                validUrns: new HashSet<int> { 123456 },
                orgType: OrganisationCategory.MultiAcademyTrust);

            var result = await _validator.ValidateAsync(context);

            Assert.That(result.Errors.Any(e =>
                e.ErrorMessage == ValidationMessages.InvalidSchoolUrnForMAT), Is.True);
        }
        [TestCase(OrganisationCategory.LocalAuthority)]
        [TestCase(OrganisationCategory.MultiAcademyTrust)]
        public async Task Validate_ValidUrnInList_Passes(OrganisationCategory orgType)
        {

            var model = new CheckEligibilityRequestData_Enhanced
            {
                NationalInsuranceNumber = "AA123456C",
                FirstName = "Test",
                LastName = "Test",
                DateOfBirth = "1985-04-23",
                Sequence = 1,

                ChildFirstName = "Emily",
                ChildLastName = "Test",
                ChildDateOfBirth = "2015-09-10",
                ChildSchoolUrn = "123456"
            };

            var context = CreateContext(
                model,
                validUrns: new HashSet<int> { 123456 },
                orgType: orgType);

            var result = await _validator.ValidateAsync(context);

            Assert.That(result.Errors, Is.Empty);
        }

        [Test]
        public async Task Validate_NoContextData_DoesNotThrowOrFail()
        {
            var model = new CheckEligibilityRequestData_Enhanced
            {
                NationalInsuranceNumber = "AA123456C",
                FirstName = "Test",
                LastName = "Test",
                DateOfBirth = "1985-04-23",
                Sequence = 1,

                ChildFirstName = "Emily",
                ChildLastName = "Test",
                ChildDateOfBirth = "2015-09-10",
                ChildSchoolUrn = "123456"
            };

            var context = new ValidationContext<CheckEligibilityRequestDataBase>(model);

            var result = await _validator.ValidateAsync(context);

            Assert.That(result.Errors, Is.Empty);
        }
        #region Helpers
        private ValidationContext<CheckEligibilityRequestDataBase> CreateContext(
            CheckEligibilityRequestData_Enhanced model,
            HashSet<int>? validUrns = null,
            OrganisationCategory? orgType = null)
        {
            var context = new ValidationContext<CheckEligibilityRequestDataBase>(model);

            if (validUrns != null)
                context.RootContextData["validSchoolUrns"] = validUrns;

            if (orgType != null)
                context.RootContextData["organisationType"] = orgType;

            return context;
        }
        #endregion
    }

}