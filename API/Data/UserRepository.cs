using System;
using System.Security.Claims;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository(DataContext context, IMapper mapper, IHttpContextAccessor _httpContextAccessor) : IUserRepository
{
    public async Task<MemberDto?> GetMemberAsync(string username)
    {
        var currentUserIdString = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var query = context.Users
            .Include(x => x.Photos)
            .AsQueryable();

        if (username == _httpContextAccessor.HttpContext?.User.Identity?.Name)
        {
            query = query.Include(x => x.Photos).IgnoreQueryFilters();
        }

        var member = await query
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

        Console.WriteLine($"Fetched MemberDto: {member}"); // Debugging
        return member;
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = context.Users.AsQueryable();

        query = query.Where(x => x.UserName != userParams.CurrentUsername);

        if(userParams.Gender != null)
        {
            query = query.Where(x => x.Gender == userParams.Gender);
        }

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive)

        };

        return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.ConfigurationProvider), userParams.PageNumber, userParams.PageSize);
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return await context.Users.Include(x => x.Photos).SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await context.Users.Include(x => x.Photos).ToListAsync();
    }

    // public async Task<Photo?> GetPhotoByIdAsync(int photoId)
    // {
    //     return await context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == photoId);
    // }

    // public async Task<bool> SaveAllAsync()
    // {
    //     return await context.SaveChangesAsync() > 0;
    // }

    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
}
