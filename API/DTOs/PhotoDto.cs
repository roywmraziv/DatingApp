namespace API.DTOs;

public class PhotoDto
{
    public int Id { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsMain { get; set; }
    public bool IsApproved { get; set; } = false;
}