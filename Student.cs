using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABEC_System.Models;

[Table("students")]
public class Student
{
    [Key]
    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("applicant_id")]
    public int ApplicantId { get; set; }

    [Column("batch_id")]
    public int BatchId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("security_pin")]
    public string SecurityPin { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("account_status")]
    public string AccountStatus { get; set; } = "Inactive";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(ApplicantId))]
    public Applicant? Applicant { get; set; }

    [ForeignKey(nameof(BatchId))]
    public Batch? Batch { get; set; }

    public ICollection<DocumentRequest> DocumentRequests { get; set; } = new List<DocumentRequest>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Archive> Archives { get; set; } = new List<Archive>();
}
