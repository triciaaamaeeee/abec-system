using ABEC_System.Constants;
using ABEC_System.Data;
using ABEC_System.DTOs;
using ABEC_System.Interfaces;
using ABEC_System.Models;
using Microsoft.EntityFrameworkCore;

namespace ABEC_System.Services;

public class BatchService : IBatchService
{
    private readonly ApplicationDbContext _db;

    public BatchService(ApplicationDbContext db) => _db = db;

    public async Task<ApiResponse<IReadOnlyList<BatchDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var batches = await _db.Batches.AsNoTracking()
            .Include(b => b.Students)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<BatchDto>>.Ok(batches.Select(Map).ToList());
    }

    public async Task<ApiResponse<BatchDto?>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var batch = await _db.Batches.AsNoTracking()
            .Include(b => b.Students)
            .FirstOrDefaultAsync(b => b.Status == SystemConstants.BatchStatuses.Active, cancellationToken);
        return ApiResponse<BatchDto?>.Ok(batch is null ? null : Map(batch));
    }

    public async Task<ApiResponse<IReadOnlyList<BatchDto>>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        var batches = await _db.Batches.AsNoTracking()
            .Include(b => b.Students)
            .Where(b => b.Status == SystemConstants.BatchStatuses.Closed)
            .OrderByDescending(b => b.EndDate)
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<BatchDto>>.Ok(batches.Select(Map).ToList());
    }

    public async Task<ApiResponse<BatchDto>> CreateAsync(CreateBatchDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.EndDate < dto.StartDate)
            return ApiResponse<BatchDto>.Fail("End date must be on or after the start date.");

        var batch = new Batch
        {
            BatchName = dto.BatchName.Trim(),
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            Status = SystemConstants.BatchStatuses.Closed
        };

        _db.Batches.Add(batch);
        await _db.SaveChangesAsync(cancellationToken);

        _db.BatchHistories.Add(new BatchHistory
        {
            BatchId = batch.BatchId,
            Action = "Batch Created",
            ActionDate = DateTime.Now
        });
        await _db.SaveChangesAsync(cancellationToken);

        return ApiResponse<BatchDto>.Ok(Map(batch), "Batch created.");
    }

    public async Task<ApiResponse<BatchDto>> ActivateAsync(int batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _db.Batches.Include(b => b.Students)
            .FirstOrDefaultAsync(b => b.BatchId == batchId, cancellationToken);
        if (batch is null)
            return ApiResponse<BatchDto>.Fail("Batch not found.");

        var currentActive = await _db.Batches
            .Where(b => b.Status == SystemConstants.BatchStatuses.Active && b.BatchId != batchId)
            .ToListAsync(cancellationToken);

        foreach (var active in currentActive)
        {
            active.Status = SystemConstants.BatchStatuses.Closed;
            _db.BatchHistories.Add(new BatchHistory
            {
                BatchId = active.BatchId,
                Action = "Batch Deactivated",
                ActionDate = DateTime.Now
            });
        }

        batch.Status = SystemConstants.BatchStatuses.Active;
        _db.BatchHistories.Add(new BatchHistory
        {
            BatchId = batch.BatchId,
            Action = "Batch Activated",
            ActionDate = DateTime.Now
        });

        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<BatchDto>.Ok(Map(batch), "Batch activated.");
    }

    public async Task<ApiResponse<BatchDto>> CompleteAsync(int batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _db.Batches.Include(b => b.Students)
            .FirstOrDefaultAsync(b => b.BatchId == batchId, cancellationToken);
        if (batch is null)
            return ApiResponse<BatchDto>.Fail("Batch not found.");

        batch.Status = SystemConstants.BatchStatuses.Closed;
        _db.BatchHistories.Add(new BatchHistory
        {
            BatchId = batch.BatchId,
            Action = "Batch Completed",
            ActionDate = DateTime.Now
        });
        await _db.SaveChangesAsync(cancellationToken);
        return ApiResponse<BatchDto>.Ok(Map(batch), "Batch completed and archived to history.");
    }

    private static BatchDto Map(Batch b) => new()
    {
        BatchId = b.BatchId,
        BatchName = b.BatchName,
        StartDate = b.StartDate,
        EndDate = b.EndDate,
        Status = b.Status,
        StudentCount = b.Students?.Count ?? 0
    };
}
