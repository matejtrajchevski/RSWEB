using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Degree { get; set; }

        [StringLength(25)]
        public string? AcademicRank { get; set; }

        [StringLength(10)]
        public string? OfficeNumber { get; set; }

        public DateTime? HireDate { get; set; }

        [StringLength(255)]
        public string? ProfilePicture { get; set; }

        public ICollection<Course>? CoursesAsFirstTeacher { get; set; }
        public ICollection<Course>? CoursesAsSecondTeacher { get; set; }
    }
}
