namespace CompressionForce.Domain.Entities;

public partial class UserLoginHistroy
{
    public int Id { get; set; }

    public string? EReventType { get; set; }

    public string? ERname { get; set; }

    public DateTime? ERdate { get; set; }

    public string? ERlevel { get; set; }

    public bool? UserStatus { get; set; }
}
