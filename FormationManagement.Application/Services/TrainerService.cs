using FormationManagement.Application.Common.Interfaces;
using FormationManagement.Application.DTOs.Trainer;
using FormationManagement.Application.Interfaces;
using DomainTrainer = FormationManagement.Domain.Entities.Trainer;

namespace FormationManagement.Application.Services;

/// <summary>Business logic for managing trainer profiles. Creation/deletion is Administrator-only; a Trainer can edit their own profile.</summary>
public class TrainerService : ITrainerService
{
    private readonly IUnitOfWork _unitOfWork;

    public TrainerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TrainerDto>> GetAllAsync()
    {
        var trainers = await _unitOfWork.Trainers.FindAsync(includeProperties: "Courses");
        return trainers.Select(ToDto).OrderBy(t => t.LastName).ToList();
    }

    public async Task<TrainerDto?> GetByIdAsync(int id)
    {
        var trainer = await _unitOfWork.Trainers.FirstOrDefaultAsync(t => t.Id == id, includeProperties: "Courses");
        return trainer is null ? null : ToDto(trainer);
    }

    public async Task<TrainerDto?> GetByUserIdAsync(string applicationUserId)
    {
        var trainer = await _unitOfWork.Trainers.FirstOrDefaultAsync(t => t.ApplicationUserId == applicationUserId, includeProperties: "Courses");
        return trainer is null ? null : ToDto(trainer);
    }

    public async Task<int> CreateAsync(TrainerUpsertDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ApplicationUserId))
            throw new ArgumentException("A Trainer profile must be linked to an existing Identity account.", nameof(dto.ApplicationUserId));

        var entity = new DomainTrainer
        {
            ApplicationUserId = dto.ApplicationUserId,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone?.Trim(),
            Biography = dto.Biography?.Trim(),
            Photo = dto.Photo
        };

        await _unitOfWork.Trainers.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(TrainerUpsertDto dto)
    {
        var entity = await _unitOfWork.Trainers.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Trainer {dto.Id} not found.");

        entity.FirstName = dto.FirstName.Trim();
        entity.LastName = dto.LastName.Trim();
        entity.Email = dto.Email.Trim();
        entity.Phone = dto.Phone?.Trim();
        entity.Biography = dto.Biography?.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Photo))
            entity.Photo = dto.Photo;

        _unitOfWork.Trainers.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Trainers.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Trainer {id} not found.");

        entity.IsDeleted = true; // soft delete: preserves course history/reporting
        _unitOfWork.Trainers.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    private static TrainerDto ToDto(DomainTrainer t) => new()
    {
        Id = t.Id,
        FirstName = t.FirstName,
        LastName = t.LastName,
        Email = t.Email,
        Phone = t.Phone,
        Biography = t.Biography,
        Photo = t.Photo,
        CourseCount = t.Courses.Count
    };
}
