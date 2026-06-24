using CheckYourEligibility.Admin.Domain.Constants.BulkCheck;
using CheckYourEligibility.Admin.Models;
using CsvHelper.Configuration;
namespace CheckYourEligibility.Admin.CsvMaps
{
    public sealed class BulkExportBaseCsvMap : ClassMap<BulkExportBase>
    {
        public BulkExportBaseCsvMap()
        {
            Map(m => m.LastName).Index(0).Name(BulkCheckConstants.ParentLastNameHeader);
            Map(m => m.DateOfBirth).Index(1).Name(BulkCheckConstants.ParentDateOfBirthHeader);
            Map(m => m.NationalInsuranceNumber).Index(2).Name(BulkCheckConstants.ParentNINOHeader);
            Map(m => m.Outcome).Index(3).Name(BulkCheckConstants.Outcome);  
            Map(m => m.EligibilityEndDate).Ignore();
        }
    }
    public sealed class BulkExportBaseExpandedCsvMap : ClassMap<BulkExportBase>
    {
        public BulkExportBaseExpandedCsvMap()
        {
            Map(m => m.LastName).Index(0).Name(BulkCheckConstants.ParentLastNameHeader);
            Map(m => m.DateOfBirth).Index(1).Name(BulkCheckConstants.ParentDateOfBirthHeader);
            Map(m => m.NationalInsuranceNumber).Index(2).Name(BulkCheckConstants.ParentNINOHeader);
            Map(m => m.Outcome).Index(3).Name(BulkCheckConstants.Outcome);
            Map(m => m.EligibilityEndDate).Index(4).Name(BulkCheckConstants.EligibilityEndDate);
        }
    }
}