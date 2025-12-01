using System;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using oop_project_coursework.Data;
using oop_project_coursework.Models;
using oop_project_coursework.Repositories;

namespace oop_project_coursework.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly AppDBContext _context;
        public IRepository<Vehicle> VehicleRepo { get; }
        public IRepository<Owner> OwnerRepo { get; }
        public IRepository<MaintenanceRecord> MaintenanceRepo { get; }

        public ObservableCollection<Vehicle> Vehicles { get; } = new();
        public ObservableCollection<Owner> Owners { get; } = new();

        private Vehicle? _selectedVehicle;
        public Vehicle? SelectedVehicle { get => _selectedVehicle; set { _selectedVehicle = value; Raise(); } }

        private Owner? _selectedOwner;
        public Owner? SelectedOwner { get => _selectedOwner; set { _selectedOwner = value; Raise(); }}

        public Vehicle NewVehicle { get; set; } = new();
        public Owner NewOwner { get; set; } = new();
        public MaintenanceRecord NewMaintenance { get; set; } =
            new MaintenanceRecord
            {
                ServiceDate = DateTime.Now,
                NextDueDate = DateTime.Now.AddMonths(6)
            };

        public MainViewModel(AppDBContext context)
        {
            _context = context;
            VehicleRepo = new Repositories.EfRepository<Vehicle>(_context);
            OwnerRepo = new Repositories.EfRepository<Owner>(_context);
            MaintenanceRepo = new Repositories.EfRepository<MaintenanceRecord>(_context);

            // Завантаження початкових даних асинхронно (fire-and-forget allowed в UI ctor)
             Task.Run(async () => await LoadDataAsync()).Wait();
        }

        public async Task LoadDataAsync()
        {
            var owners = await OwnerRepo.GetAllAsync();
            Owners.Clear();
            foreach (var o in owners) Owners.Add(o);

            // завантажимо Vehicles включно з власником і ТО
            var vehicles = await _context.Vehicles.Include(v => v.Owner).Include(v => v.MaintenanceRecords).ToListAsync();
            Vehicles.Clear();
            foreach (var v in vehicles) Vehicles.Add(v);
        }

        // Додавання власника
        public async Task AddOwnerAsync(Owner owner)
        {
            // Перевірка валідації
            if (string.IsNullOrWhiteSpace(owner.FullName))
            {
                throw new InvalidOperationException("ФІО не може бути порожнім.");
            }

            if (string.IsNullOrWhiteSpace(owner.Phone))
            {
                throw new InvalidOperationException("Телефон не може бути порожнім.");
            }

            await OwnerRepo.AddAsync(owner);
            await OwnerRepo.SaveChangesAsync();
            Owners.Add(owner);

            NewOwner = new Owner();
            Raise(nameof(NewOwner));
        }

        // Видалення власника
        public async Task DeleteOwnerAsync(Owner owner)
        {
            if (owner == null) return;

            // Перед видаленням перевіримо, чи має власник авто
            var vehiclesWithOwner = await _context.Vehicles.Where(v => v.OwnerId == owner.OwnerId).ToListAsync();
            if (vehiclesWithOwner?.Count > 0)
                throw new InvalidOperationException("Неможливо видалити власника, який має транспортні засоби.");

            await OwnerRepo.DeleteAsync(owner);
            await OwnerRepo.SaveChangesAsync();
            Owners.Remove(owner);
        }

        // Додавання авто
        public async Task AddVehicleAsync(Vehicle v)
        {
            // === Валідація ===
            if (v.OwnerId <= 0)
                throw new InvalidOperationException("Помилка: Власник не може бути відсутнім.");

            if (string.IsNullOrWhiteSpace(v.RegistrationNumber))
                throw new InvalidOperationException("Помилка: Реєстраційний номер не може бути відсутнім.");

            if (string.IsNullOrWhiteSpace(v.Make))
                throw new InvalidOperationException("Помилка: Марка не може бути відсутньою.");

            if (string.IsNullOrWhiteSpace(v.Model))
                throw new InvalidOperationException("Помилка: Модель не може бути відсутньою.");

            if (string.IsNullOrWhiteSpace(v.VehicleType))
                throw new InvalidOperationException("Помилка: Тип транспорту не може бути відсутнім.");

            await VehicleRepo.AddAsync(v);
            await VehicleRepo.SaveChangesAsync();

            // Підвантажимо власника
            var owner = await OwnerRepo.GetAsync(v.OwnerId);
            //if (owner != null)
            v.Owner = owner;
            Vehicles.Add(v);
            NewVehicle = new Vehicle();
            Raise(nameof(NewVehicle));
        }


        // Оновлення авто
        public async Task UpdateVehicleAsync(Vehicle v)
        {
            var entry = _context.Entry(v);

            // Проходимо по всіх властивостях EF Entry
            foreach (var prop in entry.Properties)
            {
                // EF позначає Modified тільки змінені властивості
                // Але нам треба вимкнути модифікацію для тих, які НЕ змінювали
                if (!prop.IsModified)
                    prop.IsModified = false;
            }

            await _context.SaveChangesAsync();
            await LoadDataAsync();
        }

        // Видалення авто
        public async Task DeleteVehicleAsync(Vehicle v)
        {
            await VehicleRepo.DeleteAsync(v);
            await VehicleRepo.SaveChangesAsync();
            Vehicles.Remove(v);
        }

        // Додавання запису ТО
        public async Task AddMaintenanceAsync(MaintenanceRecord m)
        {
            var existing = await _context.MaintenanceRecords
                .FirstOrDefaultAsync(r => r.VehicleId == m.VehicleId);

            bool needsRepair = m.NextDueDate.Date < DateTime.Today;

            if (existing != null)
            {
                existing.ServiceDate = m.ServiceDate;
                existing.NextDueDate = m.NextDueDate;
                existing.Notes = m.Notes;
                existing.NeedsRepair = needsRepair;

                await _context.SaveChangesAsync();
            }
            else
            {
                m.NeedsRepair = needsRepair;
                await MaintenanceRepo.AddAsync(m);
                await MaintenanceRepo.SaveChangesAsync();
            }
        }


        // Отримання авто з простроченим оглядом
        public async Task<List<Vehicle>> GetVehiclesWithExpiredInspectionAsync()
        {
            var today = DateTime.Today;

            var vehicles = await _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.MaintenanceRecords)
                .ToListAsync();

            foreach (var v in vehicles)
            {
                var last = v.MaintenanceRecords
                             .OrderByDescending(m => m.ServiceDate)
                             .FirstOrDefault();

                bool repair = last != null && last.NextDueDate.Date < today;

                if (repair != v.NeedsRepair)
                {
                    v.NeedsRepair = repair;
                    await VehicleRepo.UpdateAsync(v);
                }
            }

            await VehicleRepo.SaveChangesAsync();

            return vehicles.Where(v => v.NeedsRepair).ToList();
        }
        // Підрахунок статистики за типами авто
        public async Task<Dictionary<string, int>> GetStatisticsByTypeAsync()
        {
            var groups = await _context.Vehicles
                .GroupBy(v => v.VehicleType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            return groups.ToDictionary(x => x.Type.ToString(), x => x.Count);
        }

        // Зберегти список авто, що потребують ремонту (CSV)
        public async Task<string> SaveVehiclesNeedingRepairAsync(string path)
        {
            var need = await _context.Vehicles
                .Include(v => v.Owner)
                .Include(v => v.MaintenanceRecords)
                .Where(v => v.NeedsRepair)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ID, Number, Make, Model, Type, Notes");

            foreach (var v in need)
            {
                var last = v.MaintenanceRecords
                             .OrderByDescending(m => m.ServiceDate)
                             .FirstOrDefault();

                sb.AppendLine($"{v.VehicleId}, {v.RegistrationNumber}, {v.Make}, {v.Model}, " +
                              $"{v.VehicleType}, {last?.Notes}");
            }

            await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
            return path;
        }

        // Приклад LINQ-фільтра
        public async Task<List<Vehicle>> SearchVehiclesAsync(string? regPattern, string? type)
        {
            var q = _context.Vehicles.Include(v => v.Owner).AsQueryable();
            if (!string.IsNullOrWhiteSpace(regPattern))
                q = q.Where(v => EF.Functions.Like(v.RegistrationNumber, $"%{regPattern}%"));
            if (!string.IsNullOrWhiteSpace(type))
                q = q.Where(v => v.VehicleType.ToString() == type);
            return await q.ToListAsync();
        }
    }
}