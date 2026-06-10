namespace CheckYourEligibility.Admin.Domain.Constants.BulkCheck
{
    public static class BulkCheckUploadConstants
    {
        public static List<string> GuidanceItemsBasic = [
             "last name",
             "date of birth (format DD/MM/YYYY or YYYY-MM-DD)",
             "National Insurance number"
            ];
        public static List<string> GuidanceItemsEnhanced = [
            "parent first name",
            "parent last name",
            "parent date of birth (format DD/MM/YYYY or YYYY-MM-DD)",
            "parent National Insurance number",
            "child first name",
            "child last name",
            "child date of birth (format DD/MM/YYYY or YYYY-MM-DD)",
          ];
    }
}
