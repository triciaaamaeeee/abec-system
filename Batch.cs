using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABEC_System.Models;

[Table("batches")]
public class Batch
{
    [Key]
    [Column("batch_id")]
    public int BatchId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("batch_name")]
    public string BatchName { get; set; } = string.Empty;

    [Column("start_date")]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime EndDate { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "Active";

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<BatchHistory> BatchHistories { get; set; } = new List<BatchHistory>();
    public ICollection<CourseSchedule> CourseSchedules { get; set; } = new List<CourseSchedule>();
}
