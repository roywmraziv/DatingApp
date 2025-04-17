using System;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await userManager.Users
            .OrderBy(x => x.UserName)
            .Select(x => new 
            {
                x.Id,
                Username = x.UserName,
                Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        if(string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

        var selectedRoles = roles.Split(",").ToArray();

        var user = await userManager.FindByNameAsync(username);

        if(user == null) return BadRequest("User not found");

        var userRoles = await userManager.GetRolesAsync(user);

        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if(!result.Succeeded) return BadRequest("Failed to add to roles");

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if(!result.Succeeded) return BadRequest("Failed to remove from roles");

        return Ok(await userManager.GetRolesAsync(user));
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult> GetPhotosForModeration()
    {
        var photos = await userManager.Users
            .SelectMany(x => x.Photos.Where(p => !p.IsApproved))
            .IgnoreQueryFilters()
            .ToListAsync();

        return Ok(photos);
    }


    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        var photo = await userManager.Users
            .SelectMany(x => x.Photos.Where(p => p.Id == photoId))
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync();

        var user = await userManager.Users
            .Include(x => x.Photos)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Photos.Any(p => p.Id == photoId));

        if(photo == null) return NotFound("Photo not found");
        if(user == null) return BadRequest("User not found");
        if(photo.IsApproved) return BadRequest("Photo already approved");

        photo.IsApproved = true;

        bool hasMainPhoto = user.Photos.Any(x => x.IsMain);
        if(!hasMainPhoto)
        {
            photo.IsMain = true;
        }

        var result = await userManager.UpdateAsync(user);

        if(!result.Succeeded) return BadRequest("Failed to approve photo");

        return NoContent();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photo = await userManager.Users
            .SelectMany(x => x.Photos.Where(p => p.Id == photoId))
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync();

        if (photo == null) return NotFound("Photo not found");

        var user = await userManager.Users
            .Include(x => x.Photos)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Photos.Any(p => p.Id == photoId));

        if (user == null) return BadRequest("User not found");

        user.Photos.Remove(photo);

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded) return BadRequest("Failed to reject photo");

        return NoContent();
    }
}
