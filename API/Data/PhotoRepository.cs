using System;
using API.DTOs;
using API.Interfaces;
using API.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
{
    public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync()
    {
        var query = context.Photos.AsQueryable();
        query = query.Where(x => !x.IsApproved);
        var photos = await query
            .ProjectTo<PhotoForApprovalDto>(mapper.ConfigurationProvider)
            .ToListAsync();
        return photos;
    }

    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
    
    public async Task<Photo?> GetPhotoByIdAsync(int id)
    {
        return await context.Photos.FindAsync(id);
    }
}
