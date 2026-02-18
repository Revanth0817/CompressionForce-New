namespace CompressionForce.Domain.Entities;

public partial class UserManagementHistroy
{
    public int Id { get; set; }

    public string? EReventType { get; set; }

    public int? ERid { get; set; }

    public string? ERname { get; set; }

    public DateTime? ERdate { get; set; }

    public string? ERlevel { get; set; }

    public string? ERpassword { get; set; }

    public DateTime? ERexpiryDate { get; set; }

    public bool? UserStatus { get; set; }
}
