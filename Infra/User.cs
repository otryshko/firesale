using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id {get;set;}

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    public Decimal Balance { get; set; }

    
}
