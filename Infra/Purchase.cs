using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

public enum PurchaseState {
    Pending,
    Completed,
    Refunded,
}

[Index("UserId", "FireSaleItemId")]
public class Purchase{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id {get;set;}

    [Required]
    public int UserId { get; set; }

    public User User {get;set;}
    [Required]

    public int FireSaleItemId { get; set; }
    public FireSaleItem FireSaleItem {get;set;}

    [Required]
    public DateTime CreatedAtUtc {get;set;}

    [Required]
    public PurchaseState State {get;set;}

    
}