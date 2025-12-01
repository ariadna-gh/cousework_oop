using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace oop_project_coursework.Models
{
    [Table("maintenance_records")]
    public class MaintenanceRecord
    {
        [Key]
        public int MaintenanceId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [Required]
        public Vehicle Vehicle { get; set; } = null!;

        [Required]
        public DateTime NextDueDate { get; set; }

        [Required]
        public DateTime ServiceDate { get; set; }

        public bool NeedsRepair { get; set; } = false;

        public string Notes { get; set; } = string.Empty;
    }
}
