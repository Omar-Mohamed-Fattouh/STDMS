namespace STDMS.Models
{
    public static class RoleNames
    {
        public const string Administrator = "Administrator";
        public const string AcademicAdministrator = "AcademicAdministrator";
        public const string Instructor = "Instructor";
        public const string ReadOnlyStaff = "ReadOnlyStaff";

        public const string SchoolRecords =
            Administrator + "," + AcademicAdministrator + "," + ReadOnlyStaff;
        public const string Administrators = Administrator + "," + AcademicAdministrator;
        public const string AttendanceUsers =
            SchoolRecords + "," + Instructor;
        public const string AttendanceWriters =
            Administrator + "," + AcademicAdministrator + "," + Instructor;
    }
}
