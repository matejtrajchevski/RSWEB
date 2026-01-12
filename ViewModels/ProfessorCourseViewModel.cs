using UniversityManagement.Models;

namespace UniversityManagement.ViewModels
{
    public class ProfessorCourseViewModel
    {
        public Course Course { get; set; } = null!;
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new List<int>();
        public List<EnrollmentDetailViewModel> Enrollments { get; set; } = new List<EnrollmentDetailViewModel>();
    }

    public class EnrollmentDetailViewModel
    {
        public long EnrollmentId { get; set; }
        public long StudentId { get; set; }
        public string StudentIndex { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
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
        public bool IsActive => FinishDate == null;
    }
}
