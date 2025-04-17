using System;
using API.DTOs;
using API.Models;

namespace API.Interfaces;

public interface IPhotoRepository
{
    public Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotosAsync();
    public void RemovePhoto(Photo photo);
    public Task<Photo?> GetPhotoByIdAsync(int id);
}
