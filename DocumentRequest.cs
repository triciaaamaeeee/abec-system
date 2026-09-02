using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABEC_System.Models;

[Table("documentrequests")]
public class DocumentRequest
{
    [Key]
    [Column("request_id")]
    public int RequestId { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("document_type")]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    [Column("reason")]
    public string Reason { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("request_status")]
    public string RequestStatus { get; set; } = "Pending";

    [Column("request_date")]
    public DateTime RequestDate { get; set; }

    [Column("release_date")]
    public DateTime? ReleaseDate { get; set; }

    [ForeignKey(nameof(StudentId))]
    public Student? Student { get; set; }
}
