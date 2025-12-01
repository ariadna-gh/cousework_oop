using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using oop_project_coursework.Models;

[Table("owners")]
public class Owner
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OwnerId { get; set; }

    [Required(ErrorMessage = "ФІО обов'язкове")]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Телефон обов'язковий")]
    [MaxLength(50)]
    public string Phone { get; set; } = null!;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
