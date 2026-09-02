using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABEC_System.Models;

[Table("courseschedules")]
public class CourseSchedule
{
    [Key]
    [Column("schedule_id")]
    public int ScheduleId { get; set; }

    [Column("course_id")]
    public int CourseId { get; set; }

    [Column("batch_id")]
    public int BatchId { get; set; }

    [Column("schedule_date")]
    public DateTime ScheduleDate { get; set; }

    [Column("start_time")]
    public TimeSpan StartTime { get; set; }

    [Column("end_time")]
    public TimeSpan EndTime { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("room")]
    public string Room { get; set; } = string.Empty;

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    [ForeignKey(nameof(BatchId))]
    public Batch? Batch { get; set; }
}
