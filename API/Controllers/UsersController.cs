using System;
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController (IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService, IPhotoRepository photoRepository, DataContext context) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
        var users = await unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }
    
    [HttpGet("{username}")] // /api/users/{id}
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await unitOfWork.UserRepository.GetMemberAsync(username);

        if (user == null) return NotFound();

        return user; // Ensure Photos collection is included in the response
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if(user == null) return BadRequest("User not found");

        mapper.Map(memberUpdateDto, user);

        if(await unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return BadRequest("User not found");

        var result = await photoService.AddPhotoAsync(file);

        // Log the result for debugging
        Console.WriteLine($"Photo Upload Result: SecureUrl={result.SecureUrl}, PublicId={result.PublicId}");

        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl?.AbsoluteUri, // Ensure SecureUrl is used correctly
            PublicId = result.PublicId
        };

        // do not automatically set the first photo as main due to new approval logic
        // if (user.Photos.Count == 0) photo.IsMain = true; 

        user.Photos.Add(photo);

        if (await unitOfWork.Complete())
            return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, mapper.Map<PhotoDto>(photo));

        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if(user == null) return BadRequest("User not found");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if(photo == null || photo.IsMain) return BadRequest("Photo not found or already main photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

        if(currentMain != null) currentMain.IsMain = false;

        photo.IsMain = true;

        if(await unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var photo = await photoRepository.GetPhotoByIdAsync(photoId);

        if (photo == null) return NotFound("Photo not found");

        if (photo.IsMain) return BadRequest("You cannot delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        photoRepository.RemovePhoto(photo);

        if (await context.SaveChangesAsync() > 0) return Ok();

        return BadRequest("Problem deleting photo");
    }

    [HttpGet("get-user-photos")]
    public async Task<ActionResult<IEnumerable<PhotoDto>>> GetUserPhotos()
    {
        var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        if(user == null) return BadRequest("User not found");

        var photos = user.Photos.Select(photo => new PhotoDto
        {
            Id = photo.Id,
            PhotoUrl = photo.Url,
            IsMain = photo.IsMain
        }).ToList();

        return Ok(photos);
    }

}
