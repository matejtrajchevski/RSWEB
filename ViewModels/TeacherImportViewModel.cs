namespace UniversityManagement.ViewModels
{
    public class TeacherImportViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Degree { get; set; }
        public string? AcademicRank { get; set; }
        public string? OfficeNumber { get; set; }
        public DateTime? HireDate { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
