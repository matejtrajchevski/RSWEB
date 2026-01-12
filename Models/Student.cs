using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Models
{
    public class Student
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(10)]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? EnrollmentDate { get; set; }

        public int? AcquiredCredits { get; set; }

        public int? CurrentSemester { get; set; }

        [StringLength(25)]
        public string? EducationLevel { get; set; }

        [StringLength(255)]
        public string? ProfilePicture { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
