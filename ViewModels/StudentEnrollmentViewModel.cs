namespace UniversityManagement.ViewModels
{
    public class StudentEnrollmentViewModel
    {
        public long EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int CourseCredits { get; set; }
        public string? Semester { get; set; }
        public int? Year { get; set; }
        public int? ExamPoints { get; set; }
        public int? SeminalPoints { get; set; }
        public int? ProjectPoints { get; set; }
        public int? AdditionalPoints { get; set; }
        public int? TotalPoints { get; set; }
        public int? Grade { get; set; }
        public DateTime? FinishDate { get; set; }
        public string? SeminalUrl { get; set; }
        public string? ProjectUrl { get; set; }
        public string? FirstTeacherName { get; set; }
        public string? SecondTeacherName { get; set; }
    }
}
