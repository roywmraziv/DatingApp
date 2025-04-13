using System;

namespace API.DTOs;

public class PhotoForApprovalDto
{
    public int PhotoId { get; set; }
    public string? Url { get; set; }
    public string Username { get; set; }
    public bool IsApproved { get; set; } = false;
}
// This DTO is used for displaying photos that are pending approval.