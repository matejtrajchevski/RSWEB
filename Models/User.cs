using System.ComponentModel.DataAnnotations;

namespace UniversityManagement.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // "Admin", "Professor", "Student"

        // Foreign keys for linking to specific entities
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        public long? StudentId { get; set; }
        public Student? Student { get; set; }
    }
}
