namespace UniversityManagement.ViewModels
{
    public class BulkEnrollmentViewModel
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Semester { get; set; } = string.Empty; // "Зимски" или "Летен"
        public List<long> SelectedStudentIds { get; set; } = new List<long>();
        public List<StudentCheckboxViewModel> AvailableStudents { get; set; } = new List<StudentCheckboxViewModel>();
        public int? FilterSemester { get; set; }
        public string? FilterEducationLevel { get; set; }
    }

    public class StudentCheckboxViewModel
    {
        public long Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public int? CurrentSemester { get; set; }
        public string? EducationLevel { get; set; }
    }
}
