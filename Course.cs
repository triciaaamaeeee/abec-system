using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABEC_System.Models;

[Table("courses")]
public class Course
{
    [Key]
    [Column("course_id")]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(150)]
    [Column("course_name")]
    public string CourseName { get; set; } = string.Empty;

    [Column("course_description")]
    public string? CourseDescription { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("duration")]
    public string Duration { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Available";

    public ICollection<Applicant> Applicants { get; set; } = new List<Applicant>();
    public ICollection<CourseSchedule> CourseSchedules { get; set; } = new List<CourseSchedule>();
}
