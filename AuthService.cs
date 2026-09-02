using ABEC_System.Constants;
using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using ABEC_System.Models;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;

    public AuthService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<AuthResultDto>> AdminLoginAsync(AdminLoginDto dto, CancellationToken cancellationToken = default)
    {
        var login = dto.Email.Trim();
        var admin = await _db.Admins.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username == login, cancellationToken);

        if (admin is null || !VerifyPassword(dto.Password, admin.Password))
            return ApiResponse<AuthResultDto>.Fail("Invalid admin credentials.");

        return ApiResponse<AuthResultDto>.Ok(new AuthResultDto
        {
            UserId = admin.AdminId,
            Role = "Admin",
            DisplayName = admin.FullName,
            EmailOrUsername = admin.Username
        }, "Admin login successful.");
    }

    public async Task<ApiResponse<AuthResultDto>> StudentLoginAsync(StudentLoginDto dto, CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var pin = dto.SecurityPin.Trim();

        var student = await _db.Students
            .AsNoTracking()
            .Include(s => s.Applicant)
            .FirstOrDefaultAsync(s =>
                s.Applicant != null &&
                s.Applicant.Email.ToLower() == email &&
                s.SecurityPin == pin, cancellationToken);

        if (student?.Applicant is null)
            return ApiResponse<AuthResultDto>.Fail("Invalid student email or security PIN.");

        if (student.AccountStatus != SystemConstants.AccountStatuses.Active)
            return ApiResponse<AuthResultDto>.Fail("Student account is not active. Please contact the registrar.");

        return ApiResponse<AuthResultDto>.Ok(new AuthResultDto
        {
            UserId = student.StudentId,
            Role = "Student",
            DisplayName = student.Applicant.FullName,
            EmailOrUsername = student.Applicant.Email
        }, "Student login successful.");
    }

    public async Task<ApiResponse<AuthResultDto>> RegisterStudentAsync(StudentRegisterDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Password != dto.ConfirmPassword)
            return ApiResponse<AuthResultDto>.Fail("Password and confirm password do not match.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var pin = dto.SecurityPin.Trim();

        var student = await _db.Students
            .Include(s => s.Applicant)
            .FirstOrDefaultAsync(s =>
                s.Applicant != null &&
                s.Applicant.Email.ToLower() == email &&
                s.SecurityPin == pin, cancellationToken);

        if (student?.Applicant is null)
            return ApiResponse<AuthResultDto>.Fail("No approved student account matches this email and security PIN.");

        student.AccountStatus = SystemConstants.AccountStatuses.Active;
        await _db.SaveChangesAsync(cancellationToken);

        return ApiResponse<AuthResultDto>.Ok(new AuthResultDto
        {
            UserId = student.StudentId,
            Role = "Student",
            DisplayName = student.Applicant.FullName,
            EmailOrUsername = student.Applicant.Email
        }, "Student account activated successfully.");
    }

    private static bool VerifyPassword(string plain, string stored)
    {
        if (string.IsNullOrWhiteSpace(stored)) return false;
        if (stored.StartsWith("$2", StringComparison.Ordinal))
            return BCrypt.Net.BCrypt.Verify(plain, stored);
        return string.Equals(plain, stored, StringComparison.Ordinal);
    }

    public static string HashPassword(string plain) => BCrypt.Net.BCrypt.HashPassword(plain);
}
