using System.ComponentModel.DataAnnotations;

namespace EnhanzerApi.Models;

public record LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; init; } = string.Empty;
}

public record LocationDto(string LocationCode, string LocationName);

public record LoginResponseDto(string Token, DateTime ExpiresAt, string Email, IReadOnlyList<LocationDto> Locations);

public record ExternalLoginResult(bool Success, string? Message, IReadOnlyList<LocationDto> Locations);

public record SavePurchaseBillRequest
{
    [Required, MinLength(1, ErrorMessage = "At least one item is required")]
    public List<PurchaseBillItemDto> Items { get; init; } = new();
}

public record PurchaseBillItemDto
{
    [Required, MaxLength(100)] public string Item { get; init; } = string.Empty;
    [Required, MaxLength(250)] public string Batch { get; init; } = string.Empty;

    [Range(0, 1_000_000_000)] public decimal StandardCost { get; init; }
    [Range(0, 1_000_000_000)] public decimal StandardPrice { get; init; }

    [Range(1, 1_000_000, ErrorMessage = "Qty must be at least 1")] public int Qty { get; init; }
    [Range(0, 1_000_000)] public int FreeQty { get; init; }
    [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")] public decimal DiscountPercent { get; init; }
}

public record SavePurchaseBillResponse(int Id, int TotalItems, int TotalQty, decimal TotalCost, decimal TotalSelling);

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}

public class ExternalApiOptions
{
    public string Url { get; set; } = string.Empty;
    public string DeviceId { get; set; } = "D001";
}
