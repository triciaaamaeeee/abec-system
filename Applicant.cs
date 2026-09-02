using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABEC_System.Models;

[Table("applicants")]
public class Applicant
{
    [Key]
    [Column("applicant_id")]
    public int ApplicantId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("middle_name")]
    public string? MiddleName { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("gender")]
    public string Gender { get; set; } = string.Empty;

    [Column("birthdate")]
    public DateTime Birthdate { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("contact_number")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("address")]
    public string Address { get; set; } = string.Empty;

    [Column("course_id")]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("application_status")]
    public string ApplicationStatus { get; set; } = "Pending";

    [Column("application_date")]
    public DateTime ApplicationDate { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    public Student? Student { get; set; }

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [NotMapped]
    public string FullName => string.IsNullOrWhiteSpace(MiddleName)
        ? $"{FirstName} {LastName}".Trim()
        : $"{FirstName} {MiddleName} {LastName}".Trim();
}
