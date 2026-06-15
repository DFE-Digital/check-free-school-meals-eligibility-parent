using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CheckYourEligibility.Admin.Domain.Constants.BulkCheck
{
    public static class BulkCheckUploadConstants
    {
        //CSV headers
        public const string ParentLastNameHeader = "Parent Last Name";
        public const string ParentDateOfBirthHeader = "Parent Date of Birth";
        public const string ParentNINOHeader = "Parent National Insurance number";

        public const string ChildFirstNameHeader = "Child First Name";
        public const string ChildLastNameHeader = "Child Last Name";
        public const string ChildDateOfBirthHeader = "Child Date of Birth";
        public const string ChildSchoolUrnHeader = "Child School Urn";

        public static string[] Headers = {
                ParentLastNameHeader,
                ParentDateOfBirthHeader,
                ParentNINOHeader };


        public static string[] enhancedHeaders = {
                ParentLastNameHeader,
                ParentDateOfBirthHeader,
                ParentNINOHeader,
                ChildFirstNameHeader,
                ChildLastNameHeader,
                ChildDateOfBirthHeader,
                ChildSchoolUrnHeader };

        public static string[] enhancedSchoolHeaders = {
                ParentLastNameHeader,
                ParentDateOfBirthHeader,
                ParentNINOHeader,
                ChildFirstNameHeader,
                ChildLastNameHeader,
                ChildDateOfBirthHeader };

        

        public static List<string> GuidanceItemsBasic = [
             "last name",
             "date of birth (format DD/MM/YYYY or YYYY-MM-DD)",
             "National Insurance number"
            ];

        /// <summary>
        /// Display different rules depending on the
        /// type of the establishment
        /// </summary>
        /// <returns></returns>
        public static List<string> GuidanceItemsEnhanced(bool isSchool)
        {

            List<string> GuidanceItemsEnhanced = [
           "parent first name",
            "parent last name",
            "parent date of birth (format DD/MM/YYYY or YYYY-MM-DD)",
            "parent National Insurance number",
            "child first name",
            "child last name",
            "child date of birth (format DD/MM/YYYY or YYYY-MM-DD)",
          ];

            if (isSchool)
            {
                GuidanceItemsEnhanced.Add("URN of the school the child attends on a full-time basis");
            }
            return GuidanceItemsEnhanced;

        }
    }

}
