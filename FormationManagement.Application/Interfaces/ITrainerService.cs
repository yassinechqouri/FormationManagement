using FormationManagement.Application.DTOs.Trainer;

namespace FormationManagement.Application.Interfaces;

public interface ITrainerService
{
    Task<IReadOnlyList<TrainerDto>> GetAllAsync();
    Task<TrainerDto?> GetByIdAsync(int id);
    Task<TrainerDto?> GetByUserIdAsync(string applicationUserId);
    Task<int> CreateAsync(TrainerUpsertDto dto);
    Task UpdateAsync(TrainerUpsertDto dto);
    Task DeleteAsync(int id);
}
