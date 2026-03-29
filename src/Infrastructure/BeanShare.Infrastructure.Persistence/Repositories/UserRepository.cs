using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(BeanShareDbContext context) : IUserRepository
{
    private readonly BeanShareDbContext _context = context;

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .OrderBy(u => u.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<User>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<User>> GetBySystemRoleAsync(SystemRole role, CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => u.SystemRole == role)
            .OrderBy(u => u.Name)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        SystemRole? roleFilter = null,
        bool? activeFilter = null,
        CancellationToken ct = default)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        if (roleFilter.HasValue)
        {
            query = query.Where(u => u.SystemRole == roleFilter.Value);
        }

        if (activeFilter.HasValue)
        {
            query = query.Where(u => u.IsActive == activeFilter.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var users = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (users, totalCount);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(UserId id, CancellationToken ct = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Users.CountAsync(ct);
    }

    public async Task<int> CountActiveAsync(CancellationToken ct = default)
    {
        return await _context.Users.CountAsync(u => u.IsActive, ct);
    }

    public async Task<int> CountByRoleAsync(SystemRole role, CancellationToken ct = default)
    {
        return await _context.Users.CountAsync(u => u.SystemRole == role, ct);
    }
}
