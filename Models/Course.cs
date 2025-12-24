using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversityManagement.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public int Credits { get; set; }

        public int Semester { get; set; }

        [StringLength(100)]
        public string? Programme { get; set; }

        [StringLength(25)]
        public string? EducationLevel { get; set; }

        [ForeignKey("FirstTeacher")]
        public int? FirstTeacherId { get; set; }
        public Teacher? FirstTeacher { get; set; }

        [ForeignKey("SecondTeacher")]
        public int? SecondTeacherId { get; set; }
        public Teacher? SecondTeacher { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
