using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Core;
using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Model;
using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.View;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.ViewModel
{
    public class ClaimsViewModel : INotifyPropertyChanged
    {
        private readonly string connectionString =
            @"Server=THOMAS;Database=Catch Catch with the TV;Trusted_Connection=True;TrustServerCertificate=True;";

        // ===================== COLLECTIONS =====================

        public ObservableCollection<Claim> ClaimsList { get; set; } = new();

        private ObservableCollection<Claim> _filteredClaims = new();
        public ObservableCollection<Claim> FilteredClaims
        {
            get => _filteredClaims;
            set { _filteredClaims = value; OnPropertyChanged(); }
        }

        // ===================== SELECTED CLAIM =====================

        private Claim _selectedClaim;
        public Claim SelectedClaim
        {
            get => _selectedClaim;
            set
            {
                _selectedClaim = value;
                OnPropertyChanged();
                // CommandManager picks this up automatically via RequerySuggested
            }
        }

        // ===================== FILTER =====================

        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        // ===================== COMMANDS =====================

        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }

        // ===================== CONSTRUCTOR =====================

        public ClaimsViewModel()
        {
            ApproveCommand = new RelayCommand(async _ => await UpdateStatus("Approved"), _ => SelectedClaim != null);
            RejectCommand = new RelayCommand(async _ => await UpdateStatus("Rejected"), _ => SelectedClaim != null);
            DeleteCommand = new RelayCommand(async _ => await DeleteClaim(), _ => SelectedClaim != null);
            BackCommand = new RelayCommand(_ => Back());

            _ = Load();
        }

        // ===================== LOAD =====================

        private async Task Load()
        {
            ClaimsList.Clear();

            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM Claims ORDER BY ClaimID DESC", conn);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ClaimsList.Add(new Claim
                {
                    ClaimID = Convert.ToInt32(reader["ClaimID"]),
                    LostItemID = Convert.ToInt32(reader["LostItemID"]),
                    StudentID = Convert.ToInt32(reader["StudentID"]),
                    VerificationStatus = reader["VerificationStatus"].ToString()
                });
            }

            ApplyFilter();
        }

        // ===================== FILTER =====================

        private void ApplyFilter()
        {
            var temp = new ObservableCollection<Claim>();

            foreach (var c in ClaimsList)
            {
                if (string.IsNullOrWhiteSpace(FilterText) ||
                    c.VerificationStatus.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                    c.ClaimID.ToString().Contains(FilterText) ||
                    c.StudentID.ToString().Contains(FilterText) ||
                    c.LostItemID.ToString().Contains(FilterText))
                {
                    temp.Add(c);
                }
            }

            FilteredClaims = temp;
        }

        // ===================== APPROVE / REJECT =====================

        private async Task UpdateStatus(string status)
        {
            if (SelectedClaim == null) return;

            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                "UPDATE Claims SET VerificationStatus=@status WHERE ClaimID=@id", conn);

            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@id", SelectedClaim.ClaimID);

            await cmd.ExecuteNonQueryAsync();

            MessageBox.Show($"Claim {SelectedClaim.ClaimID} marked as {status}.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            await Load();
        }

        // ===================== DELETE =====================

        private async Task DeleteClaim()
        {
            if (SelectedClaim == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete Claim ID {SelectedClaim.ClaimID}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                "DELETE FROM Claims WHERE ClaimID=@id", conn);

            cmd.Parameters.AddWithValue("@id", SelectedClaim.ClaimID);

            await cmd.ExecuteNonQueryAsync();

            await Load();
        }

        // ===================== BACK =====================

        private void Back()
        {
            var dashboard = new AdminDashboard();
            dashboard.Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w is Claims)
                {
                    w.Close();
                    break;
                }
            }
        }

        // ===================== INPC =====================

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}