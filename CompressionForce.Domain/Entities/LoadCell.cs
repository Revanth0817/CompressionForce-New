using System.ComponentModel.DataAnnotations;

public class LoadCell
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string LoadCellCode { get; set; }

    [Required]
    [MaxLength(100)]
    public string LoadCellName { get; set; }

    [MaxLength(10)]
    public string Unit { get; set; }

    public bool IsActive { get; set; } = true;
}
