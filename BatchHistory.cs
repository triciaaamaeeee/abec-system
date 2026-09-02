using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABEC_System.Models;

[Table("batchhistory")]
public class BatchHistory
{
    [Key]
    [Column("history_id")]
    public int HistoryId { get; set; }

    [Column("batch_id")]
    public int BatchId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("action")]
    public string Action { get; set; } = string.Empty;

    [Column("action_date")]
    public DateTime ActionDate { get; set; }

    [ForeignKey(nameof(BatchId))]
    public Batch? Batch { get; set; }
}
