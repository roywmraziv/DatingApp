using System;

namespace API.Helpers;

public class PaginationHeader(int CurrentPage, int itemsPerPage, int totalItems, int totalPages)
{
    public int CurrentPage { get; set; } = CurrentPage;
    public int ItemsPerPage { get; set; } = itemsPerPage;
    public int TotalItems { get; set; } = totalItems;
    public int TotalPages { get; set; } = totalPages;
}
