using System;
using API.Data;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(ILikesRepository likesRepository) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        var sourceUserId = User.GetUserId();

        if(sourceUserId == targetUserId) return BadRequest("Cannot like yourself");

        var existingLike = await likesRepository.GetUserLike(sourceUserId, targetUserId);

        if(existingLike == null)
        {
            var like = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId,
            };
            likesRepository.AddLike(like);
        }
        else
        {
            likesRepository.DeleteLike(existingLike);
        }

        if(await likesRepository.SaveChanges()) return Ok();

        return BadRequest("Failed to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds(){
        return Ok(await likesRepository.GetCurrentUserLikeIds(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikesParams likeParams)
    {   
        likeParams.UserId = User.GetUserId();
        var users = await likesRepository.GetUserLikes(likeParams);
        Response.AddPaginationHeader(users);
        return Ok(users);
    }
}
