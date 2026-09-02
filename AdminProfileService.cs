using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class AdminProfileService : IAdminProfileService
{
    private readonly ApplicationDbContext _db;

    public AdminProfileService(ApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<AdminProfileDto>> GetProfileAsync(int adminId, CancellationToken cancellationToken = default)
    {
        var admin = await _db.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.AdminId == adminId, cancellationToken);
        return admin is null
            ? ApiResponse<AdminProfileDto>.Fail("Admin not found.")
            : ApiResponse<AdminProfileDto>.Ok(Map(admin));
    }

    public async Task<ApiResponse<AdminProfileDto>> UpdateProfileAsync(int adminId, UpdateAdminProfileDto dto, CancellationToken cancellationToken = default)
    {
        var admin = await _db.Admins.Include(a => a.UserSetting)
            .FirstOrDefaultAsync(a => a.AdminId == adminId, cancellationToken);
        if (admin is null)
            return ApiResponse<AdminProfileDto>.Fail("Admin not found.");

        var usernameTaken = await _db.Admins.AnyAsync(a => a.Username == dto.Username && a.AdminId != adminId, cancellationToken);
        if (usernameTaken)
            return ApiResponse<AdminProfileDto>.Fail("Username is already in use.");

        admin.FullName = dto.FullName.Trim();
        admin.Username = dto.Username.Trim();

        if (admin.UserSetting is not null)
        {
            admin.UserSetting.FullName = admin.FullName;
            admin.UserSetting.Username = admin.Username;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<AdminProfileDto>.Ok(Map(admin), "Profile updated.");
    }

    public async Task<ApiResponse<object>> ChangePasswordAsync(int adminId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var admin = await _db.Admins.Include(a => a.UserSetting)
            .FirstOrDefaultAsync(a => a.AdminId == adminId, cancellationToken);
        if (admin is null)
            return ApiResponse<object>.Fail("Admin not found.");

        if (!VerifyPassword(dto.CurrentPassword, admin.Password))
            return ApiResponse<object>.Fail("Current password is incorrect.");

        var hashed = AuthService.HashPassword(dto.NewPassword);
        admin.Password = hashed;
        if (admin.UserSetting is not null)
            admin.UserSetting.Password = hashed;

        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { updated = true }, "Password changed successfully.");
    }

    private static bool VerifyPassword(string plain, string stored)
    {
        if (stored.StartsWith("$2", StringComparison.Ordinal))
            return BCrypt.Net.BCrypt.Verify(plain, stored);
        return string.Equals(plain, stored, StringComparison.Ordinal);
    }

    private static AdminProfileDto Map(Models.Admin a) => new()
    {
        AdminId = a.AdminId,
        FullName = a.FullName,
        Username = a.Username,
        CreatedAt = a.CreatedAt
    };
}
