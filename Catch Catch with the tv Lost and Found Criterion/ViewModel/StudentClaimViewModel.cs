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
    public class StudentClaimViewModel : INotifyPropertyChanged
    {
        private readonly string connectionString =
            @"Server=THOMAS;Database=Catch Catch with the TV;Trusted_Connection=True;TrustServerCertificate=True;";

        // ===================== COLLECTIONS =====================

        public ObservableCollection<LostItem> AllItems { get; set; } = new();

        private ObservableCollection<LostItem> _availableItems = new();
        public ObservableCollection<LostItem> AvailableItems
        {
            get => _availableItems;
            set { _availableItems = value; OnPropertyChanged(); }
        }

        // ===================== SELECTED ITEM =====================

        private LostItem _selectedItem;
        public LostItem SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        // ===================== STUDENT ID =====================

        public int StudentID => SharedData.CurrentStudentID ?? 0;

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

        // ===================== CLAIM FIELDS =====================

        private string _claimedColor = "";
        public string ClaimedColor
        {
            get => _claimedColor;
            set { _claimedColor = value; OnPropertyChanged(); }
        }

        private string _claimedLocationLost = "";
        public string ClaimedLocationLost
        {
            get => _claimedLocationLost;
            set { _claimedLocationLost = value; OnPropertyChanged(); }
        }

        private string _claimDescription = "";
        public string ClaimDescription
        {
            get => _claimDescription;
            set { _claimDescription = value; OnPropertyChanged(); }
        }

        private DateTime? _claimedDateLost = DateTime.Now;
        public DateTime? ClaimedDateLost
        {
            get => _claimedDateLost;
            set { _claimedDateLost = value; OnPropertyChanged(); }
        }

        // ===================== COMMANDS =====================

        public ICommand SubmitClaimCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BackCommand { get; }

        // ===================== CONSTRUCTOR =====================

        public StudentClaimViewModel()
        {
            SubmitClaimCommand = new RelayCommand(async _ => await SubmitClaim());
            ClearCommand = new RelayCommand(_ => ClearFields());
            BackCommand = new RelayCommand(_ => Back());

            _ = LoadAvailableItems();
        }

        // ===================== LOAD =====================

        private async Task LoadAvailableItems()
        {
            AllItems.Clear();

            try
            {
                using SqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                var cmd = new SqlCommand(@"
                    SELECT LostItemID, ItemName,
                           ISNULL(ItemDescription, '') AS ItemDescription,
                           ISNULL(LocationFound, '')   AS LocationFound,
                           DateFound, IsFound, EmployeeID
                    FROM Lost_Items
                    WHERE IsFound = 0
                    ORDER BY DateFound DESC", conn);

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    AllItems.Add(new LostItem
                    {
                        LostItemID = Convert.ToInt32(reader["LostItemID"]),
                        ItemName = reader["ItemName"].ToString() ?? "",
                        ItemDescription = reader["ItemDescription"].ToString() ?? "",
                        LocationFound = reader["LocationFound"].ToString() ?? "",
                        DateFound = Convert.ToDateTime(reader["DateFound"]),
                        IsFound = Convert.ToBoolean(reader["IsFound"]),
                        EmployeeID = reader["EmployeeID"] == DBNull.Value
                                            ? 0
                                            : Convert.ToInt32(reader["EmployeeID"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load available items.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ApplyFilter();
        }

        // ===================== FILTER =====================

        private void ApplyFilter()
        {
            var temp = new ObservableCollection<LostItem>();

            foreach (var item in AllItems)
            {
                if (string.IsNullOrWhiteSpace(FilterText) ||
                    item.ItemName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                    item.ItemDescription.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                    item.LocationFound.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                {
                    temp.Add(item);
                }
            }

            AvailableItems = temp;
        }

        // ===================== SUBMIT CLAIM =====================

        private async Task SubmitClaim()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Please select an item to claim.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SharedData.CurrentStudentID == null || SharedData.CurrentStudentID == 0)
            {
                MessageBox.Show("No student profile is linked to this account.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(ClaimedColor) ||
                string.IsNullOrWhiteSpace(ClaimedLocationLost) ||
                string.IsNullOrWhiteSpace(ClaimDescription) ||
                ClaimedDateLost == null)
            {
                MessageBox.Show("Please complete all claim verification fields.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using SqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                var checkCmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM Claims
                    WHERE LostItemID         = @LostItemID
                    AND   StudentID          = @StudentID
                    AND   VerificationStatus = 'Pending'", conn);

                checkCmd.Parameters.AddWithValue("@LostItemID", SelectedItem.LostItemID);
                checkCmd.Parameters.AddWithValue("@StudentID", SharedData.CurrentStudentID.Value);

                int existing = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

                if (existing > 0)
                {
                    MessageBox.Show("You already have a pending claim for this item.", "Duplicate",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var cmd = new SqlCommand(@"
                    INSERT INTO Claims
                        (LostItemID, StudentID, VerificationStatus, DateClaimed)
                    VALUES
                        (@LostItemID, @StudentID, 'Pending', GETDATE())", conn);

                cmd.Parameters.AddWithValue("@LostItemID", SelectedItem.LostItemID);
                cmd.Parameters.AddWithValue("@StudentID", SharedData.CurrentStudentID.Value);

                await cmd.ExecuteNonQueryAsync();

                MessageBox.Show("Claim submitted successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ClearFields();
                await LoadAvailableItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to submit claim.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ===================== CLEAR =====================

        private void ClearFields()
        {
            ClaimedColor = "";
            ClaimedLocationLost = "";
            ClaimDescription = "";
            ClaimedDateLost = DateTime.Now;
            SelectedItem = null;
        }

        // ===================== BACK =====================

        private void Back()
        {
            new StudentDashboard().Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w is StudentClaim)
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