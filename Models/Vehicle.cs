using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace oop_project_coursework.Models
{
    public class Vehicle
    {
        [Column("VehicleId")]
        public int VehicleId { get; set; }

        public string RegistrationNumber { get; set; } = null!;
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string VehicleType { get; set; } = null!;

        [Column("OwnerId")]
        public int OwnerId { get; set; }

        public Owner? Owner { get; set; } = null!;

        public bool NeedsRepair { get; set; } = false;

        public string Notes { get; set; } = string.Empty;

        public List<MaintenanceRecord> MaintenanceRecords { get; set; } = new();
    }
}
