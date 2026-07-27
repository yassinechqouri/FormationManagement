using FormationManagement.Application.Common.Interfaces;
using FormationManagement.Application.Common.Models;
using FormationManagement.Application.DTOs.Course;
using FormationManagement.Application.Interfaces;
using DomainCourse = FormationManagement.Domain.Entities.Course;

namespace FormationManagement.Application.Services;

/// <summary>
/// Business logic for the course catalog: CRUD used by Admin/Trainer screens,
/// plus the search/filter/sort/paginate query used by the public catalog.
/// </summary>
public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CourseDto>> SearchAsync(CourseFilterDto filter)
    {
        var term = filter.SearchTerm?.Trim().ToLower();

        // Single combined predicate — EF Core translates the whole expression
        // to one SQL WHERE clause, so filters can be freely mixed and matched.
        System.Linq.Expressions.Expression<Func<DomainCourse, bool>> predicate = c =>
            (string.IsNullOrEmpty(term) || c.Title.ToLower().Contains(term) || c.Description.ToLower().Contains(term)) &&
            (filter.CategoryId == null || c.CategoryId == filter.CategoryId) &&
            (filter.TrainerId == null || c.TrainerId == filter.TrainerId) &&
            (filter.Level == null || c.Level == filter.Level) &&
            (filter.PublishedOnly != true || c.Published);

        var totalCount = await _unitOfWork.Courses.CountAsync(predicate);

        Func<IQueryable<DomainCourse>, IOrderedQueryable<DomainCourse>> orderBy = filter.SortBy switch
        {
            CourseSortOption.Oldest => q => q.OrderBy(c => c.CreatedAt),
            CourseSortOption.TitleAsc => q => q.OrderBy(c => c.Title),
            CourseSortOption.TitleDesc => q => q.OrderByDescending(c => c.Title),
            CourseSortOption.PriceAsc => q => q.OrderBy(c => c.Price),
            CourseSortOption.PriceDesc => q => q.OrderByDescending(c => c.Price),
            CourseSortOption.MostPopular => q => q.OrderByDescending(c => c.Enrollments.Count),
            _ => q => q.OrderByDescending(c => c.CreatedAt) // Newest (default)
        };

        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize < 1 ? 9 : filter.PageSize;

        var courses = await _unitOfWork.Courses.FindAsync(
            filter: predicate,
            orderBy: orderBy,
            includeProperties: "Category,Trainer,Enrollments,Modules",
            skip: (pageNumber - 1) * pageSize,
            take: pageSize);

        return new PagedResult<CourseDto>
        {
            Items = courses.Select(ToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CourseDto?> GetByIdAsync(int id)
    {
        var course = await _unitOfWork.Courses.FirstOrDefaultAsync(c => c.Id == id, includeProperties: "Category,Trainer,Enrollments,Modules");
        return course is null ? null : ToDto(course);
    }

    public async Task<IReadOnlyList<CourseDto>> GetByTrainerAsync(int trainerId)
    {
        var courses = await _unitOfWork.Courses.FindAsync(
            filter: c => c.TrainerId == trainerId,
            orderBy: q => q.OrderByDescending(c => c.CreatedAt),
            includeProperties: "Category,Trainer,Enrollments,Modules");

        return courses.Select(ToDto).ToList();
    }

    public async Task<int> CreateAsync(CourseUpsertDto dto)
    {
        var entity = new DomainCourse
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            CategoryId = dto.CategoryId,
            TrainerId = dto.TrainerId,
            Level = dto.Level,
            Price = dto.Price,
            Duration = dto.Duration,
            Thumbnail = dto.Thumbnail,
            Published = dto.Published
        };

        await _unitOfWork.Courses.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(CourseUpsertDto dto)
    {
        var entity = await _unitOfWork.Courses.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Course {dto.Id} not found.");

        entity.Title = dto.Title.Trim();
        entity.Description = dto.Description.Trim();
        entity.CategoryId = dto.CategoryId;
        entity.TrainerId = dto.TrainerId;
        entity.Level = dto.Level;
        entity.Price = dto.Price;
        entity.Duration = dto.Duration;
        entity.Published = dto.Published;

        if (!string.IsNullOrWhiteSpace(dto.Thumbnail))
            entity.Thumbnail = dto.Thumbnail;

        _unitOfWork.Courses.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Courses.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Course {id} not found.");

        entity.IsDeleted = true; // soft delete: preserves enrollment/progress history
        _unitOfWork.Courses.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetPublishedAsync(int id, bool published)
    {
        var entity = await _unitOfWork.Courses.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Course {id} not found.");

        entity.Published = published;
        _unitOfWork.Courses.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    private static CourseDto ToDto(DomainCourse c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Description = c.Description,
        CategoryId = c.CategoryId,
        CategoryName = c.Category?.Name ?? string.Empty,
        TrainerId = c.TrainerId,
        TrainerName = c.Trainer?.FullName ?? string.Empty,
        Level = c.Level,
        Price = c.Price,
        Duration = c.Duration,
        Thumbnail = c.Thumbnail,
        Published = c.Published,
        EnrollmentCount = c.Enrollments.Count,
        ModuleCount = c.Modules.Count
    };
}
