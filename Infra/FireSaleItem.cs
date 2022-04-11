using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class FireSaleItem {

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id {get;set;}

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    public Decimal Price {get;set;}

    [Required]
    public int Quantity {get;set;}

    [Required]
    public DateTime BeginAtUtc {get;set;}

}