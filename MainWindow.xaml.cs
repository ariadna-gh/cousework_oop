using System.IO;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using oop_project_coursework.Data;
using oop_project_coursework.Models;
using oop_project_coursework.Repositories;
using oop_project_coursework.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace oop_project_coursework
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            // Налаштування DbContext з SQLite (файл app.db в папці запуску)
            var options = new DbContextOptionsBuilder<AppDBContext>().UseMySql("Server=localhost;Database=coursework_oop;User=root;Password=password;",
                new MySqlServerVersion(new Version(8, 0, 39))).Options;

            var ctx = new AppDBContext(options);


            // Автоматичне створення БД (для розробки). У продакшн краще використовувати міграції.
            ctx.Database.Migrate();

            _vm = new MainViewModel(ctx);
            DataContext = _vm;
        }

        // Кнока додавання власника
        private async void AddOwner_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _vm.AddOwnerAsync(_vm.NewOwner);
                InfoText.Text = "Власника додано.";
            }
            catch (Exception ex)
            {
                InfoText.Text = $"Помилка: {ex.Message}";
            }
        }

        // Кнопка видалення власника
        private async void DeleteOwner_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedOwner == null)
            {
                InfoText.Text = "Не обрано власника.";
                return;
            }

            try
            {
                await _vm.DeleteOwnerAsync(_vm.SelectedOwner);
                InfoText.Text = "Власника видалено.";
            }
            catch (Exception ex)
            {
                InfoText.Text = $"Помилка: {ex.Message}";
            }
        }

        // Кнопка додавання транспортного засобу
        private async void AddVehicle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _vm.AddVehicleAsync(_vm.NewVehicle);
                InfoText.Text = "Транспорт додано.";
            }
            catch (Exception ex) { InfoText.Text = ex.Message; }
        }

        // Кнопка оновлення транспортного засобу
        private async void UpdateVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedVehicle != null)
            {
                await _vm.UpdateVehicleAsync(_vm.SelectedVehicle);
                InfoText.Text = "Оновлено.";
            }
        }

        // Кнопка видалення транспортного засобу
        private async void DeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.SelectedVehicle != null)
            {
                await _vm.DeleteVehicleAsync(_vm.SelectedVehicle);
                InfoText.Text = "Видалено.";
            }
        }

        // Кнопка додавання запису ТО
        private async void AddMaintenance_Click(object sender, RoutedEventArgs e)
        {
            if (_vm.NewMaintenance.VehicleId == 0)
            {
                InfoText.Text = "Оберіть авто для ТО.";
                return;
            }

            if (_vm.NewMaintenance.ServiceDate == default)
                _vm.NewMaintenance.ServiceDate = DateTime.Now;

            try
            {
                await _vm.AddMaintenanceAsync(_vm.NewMaintenance);
                InfoText.Text = "Запис ТО додано.";
            }
            catch (Exception ex)
            {
                InfoText.Text = $"Помилка: {ex.Message}";
            }
        }

        //Кнопка пошуку прострочених оглядів
        private async void FindExpired_Click(object sender, RoutedEventArgs e)
        {
            var expired = await _vm.GetVehiclesWithExpiredInspectionAsync();
            if (expired.Count == 0)
            {
                InfoText.Text = "Прострочених оглядів не знайдено.";
            }
            else
            {
                InfoText.Text = $"Прострочені: {string.Join(", ", expired.Select(v => v.RegistrationNumber))}";

                // Оновлюємо DataGrid
                _vm.Vehicles.Clear();
                foreach (var v in expired) _vm.Vehicles.Add(v);
            }
        }

        //Показати статистику по типах авто
        private async void ShowStats_Click(object sender, RoutedEventArgs e)
        {
            var dict = await _vm.GetStatisticsByTypeAsync();
            InfoText.Text = string.Join("; ", dict.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
        }

        //Кнопка експорту авто, що потребують ремонту
        private async void ExportRepair_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "vehicles_needing_repair.csv");
            var saved = await _vm.SaveVehiclesNeedingRepairAsync(path);
            InfoText.Text = $"Експортовано у {saved}";
        }

        // Обробка вибору авто в DataGrid
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedVehicle = (Vehicle)((DataGrid)sender).SelectedItem;
        }

        // Обробка вибору власника в DataGrid
        private void OwnersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.SelectedOwner = (Owner)((DataGrid)sender).SelectedItem;
        }
    }
}