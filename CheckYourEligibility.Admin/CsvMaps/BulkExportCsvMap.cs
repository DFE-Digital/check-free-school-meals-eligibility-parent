using CheckYourEligibility.Admin.Models;
using CheckYourEligibility.Admin.Domain.Constants.BulkCheck;
using CsvHelper.Configuration;

namespace CheckYourEligibility.Admin.CsvMaps
{
    public sealed class BulkExportCsvMap : ClassMap<BulkExport>
    {
        public BulkExportCsvMap()
        {
            Map(m => m.FirstName).Index(0).Name(BulkCheckConstants.ParentFirstNameHeader);
            Map(m => m.LastName).Index(1).Name(BulkCheckConstants.ParentLastNameHeader);
            Map(m => m.DateOfBirth).Index(2).Name(BulkCheckConstants.ParentDateOfBirthHeader);
            Map(m => m.NationalInsuranceNumber).Index(3).Name(BulkCheckConstants.ParentNINOHeader);
            Map(m => m.ChildFirstName).Index(4).Name(BulkCheckConstants.ChildFirstNameHeader);
            Map(m => m.ChildLastName).Index(5).Name(BulkCheckConstants.ChildLastNameHeader);
            Map(m => m.ChildDateOfBirth).Index(6).Name(BulkCheckConstants.ChildDateOfBirthHeader);
            Map(m => m.ChildSchoolUrn).Index(7).Name(BulkCheckConstants.ChildSchoolUrnHeader);
            Map(m => m.Outcome).Index(8).Name(BulkCheckConstants.Outcome);  
            Map(m => m.EligibilityEndDate).Ignore();
        }
    }
    public sealed class BulkExportExpandedCsvMap : ClassMap<BulkExport>
    {
        public BulkExportExpandedCsvMap()
        {
            Map(m => m.FirstName).Index(0).Name(BulkCheckConstants.ParentFirstNameHeader);
            Map(m => m.LastName).Index(1).Name(BulkCheckConstants.ParentLastNameHeader);
            Map(m => m.DateOfBirth).Index(2).Name(BulkCheckConstants.ParentDateOfBirthHeader);
            Map(m => m.NationalInsuranceNumber).Index(3).Name(BulkCheckConstants.ParentNINOHeader);
            Map(m => m.ChildFirstName).Index(4).Name(BulkCheckConstants.ChildFirstNameHeader);
            Map(m => m.ChildLastName).Index(5).Name(BulkCheckConstants.ChildLastNameHeader);
            Map(m => m.ChildDateOfBirth).Index(6).Name(BulkCheckConstants.ChildDateOfBirthHeader);
            Map(m => m.ChildSchoolUrn).Index(7).Name(BulkCheckConstants.ChildSchoolUrnHeader);
            Map(m => m.EligibilityEndDate).Index(9).Name(BulkCheckConstants.ChildDateOfBirthHeader);
            Map(m => m.Outcome).Index(8).Name(BulkCheckConstants.Outcome);
            Map(m => m.EligibilityEndDate).Index(9).Name(BulkCheckConstants.EligibilityEndDate);
        }
    }
}