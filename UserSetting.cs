using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABEC_System.Models;

[Table("usersettings")]
public class UserSetting
{
    [Key]
    [Column("setting_id")]
    public int SettingId { get; set; }

    [Column("admin_id")]
    public int AdminId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("fullname")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [ForeignKey(nameof(AdminId))]
    public Admin? Admin { get; set; }
}
