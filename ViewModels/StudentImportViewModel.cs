namespace UniversityManagement.ViewModels
{
    public class StudentImportViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? EnrollmentDate { get; set; }
        public int CurrentSemester { get; set; }
        public int AcquiredCredits { get; set; }
        public string? EducationLevel { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
