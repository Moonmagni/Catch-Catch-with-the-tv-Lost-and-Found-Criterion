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
    public class LostItemsViewModel : INotifyPropertyChanged
    {
        private readonly string connectionString =
            @"Server=THOMAS;Database=Catch Catch with the TV;Trusted_Connection=True;TrustServerCertificate=True;";

        // ===================== COLLECTIONS =====================

        public ObservableCollection<LostItem> Items { get; set; } = new();

        private ObservableCollection<LostItem> _filteredItems = new();
        public ObservableCollection<LostItem> FilteredItems
        {
            get => _filteredItems;
            set { _filteredItems = value; OnPropertyChanged(); }
        }

        // ===================== SELECTED ITEM =====================

        private LostItem _selectedItem;
        public LostItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();

                // When user clicks a row, copy it into the form fields
                if (_selectedItem != null)
                {
                    NewItem = new LostItem
                    {
                        LostItemID = _selectedItem.LostItemID,
                        ItemName = _selectedItem.ItemName,
                        ItemDescription = _selectedItem.ItemDescription,
                        LocationFound = _selectedItem.LocationFound,
                        DateFound = _selectedItem.DateFound,
                        IsFound = _selectedItem.IsFound,
                        EmployeeID = _selectedItem.EmployeeID
                    };
                }
                // CommandManager picks this up automatically via RequerySuggested
            }
        }

        // ===================== FORM BINDING =====================

        private LostItem _newItem = new();
        public LostItem NewItem
        {
            get => _newItem;
            set { _newItem = value; OnPropertyChanged(); }
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

        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BackCommand { get; }

        // ===================== CONSTRUCTOR =====================

        public LostItemsViewModel()
        {
            SaveCommand = new RelayCommand(async _ => await Save());
            UpdateCommand = new RelayCommand(async _ => await Update(), _ => SelectedItem != null);
            DeleteCommand = new RelayCommand(async _ => await Delete(), _ => SelectedItem != null);
            ClearCommand = new RelayCommand(_ => Clear());
            BackCommand = new RelayCommand(_ => Back());

            _ = Load();
        }

        // ===================== LOAD =====================

        private async Task Load()
        {
            Items.Clear();

            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM Lost_Items ORDER BY LostItemID DESC", conn);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Items.Add(new LostItem
                {
                    LostItemID = Convert.ToInt32(reader["LostItemID"]),
                    ItemName = reader["ItemName"].ToString(),
                    ItemDescription = reader["ItemDescription"].ToString(),
                    LocationFound = reader["LocationFound"].ToString(),
                    DateFound = Convert.ToDateTime(reader["DateFound"]),
                    IsFound = Convert.ToBoolean(reader["IsFound"]),
                    EmployeeID = reader["EmployeeID"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(reader["EmployeeID"])
                });
            }

            ApplyFilter();
        }

        // ===================== FILTER =====================

        private void ApplyFilter()
        {
            var temp = new ObservableCollection<LostItem>();

            foreach (var item in Items)
            {
                if (string.IsNullOrWhiteSpace(FilterText) ||
                    item.ItemName.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                    item.ItemDescription.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ||
                    item.LocationFound.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                {
                    temp.Add(item);
                }
            }

            // Reassign so FilteredItems raises PropertyChanged and DataGrid refreshes
            FilteredItems = temp;
        }

        // ===================== SAVE =====================

        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(NewItem.ItemName))
            {
                MessageBox.Show("Item Name is required.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                INSERT INTO Lost_Items
                    (ItemName, ItemDescription, LocationFound, DateFound, IsFound, EmployeeID)
                VALUES
                    (@name, @desc, @loc, @date, @found, @emp)", conn);

            cmd.Parameters.AddWithValue("@name", NewItem.ItemName ?? "");
            cmd.Parameters.AddWithValue("@desc", NewItem.ItemDescription ?? "");
            cmd.Parameters.AddWithValue("@loc", NewItem.LocationFound ?? "");
            cmd.Parameters.AddWithValue("@date", DateTime.Now);
            cmd.Parameters.AddWithValue("@found", NewItem.IsFound);
            cmd.Parameters.AddWithValue("@emp", NewItem.EmployeeID);

            await cmd.ExecuteNonQueryAsync();

            MessageBox.Show("Item saved successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            await Load();
            Clear();
        }

        // ===================== UPDATE =====================

        private async Task Update()
        {
            if (SelectedItem == null) return;

            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                UPDATE Lost_Items
                SET ItemName        = @name,
                    ItemDescription = @desc,
                    LocationFound   = @loc,
                    IsFound         = @found,
                    EmployeeID      = @emp
                WHERE LostItemID = @id", conn);

            cmd.Parameters.AddWithValue("@id", SelectedItem.LostItemID);
            cmd.Parameters.AddWithValue("@name", NewItem.ItemName ?? "");
            cmd.Parameters.AddWithValue("@desc", NewItem.ItemDescription ?? "");
            cmd.Parameters.AddWithValue("@loc", NewItem.LocationFound ?? "");
            cmd.Parameters.AddWithValue("@found", NewItem.IsFound);
            cmd.Parameters.AddWithValue("@emp", NewItem.EmployeeID);

            await cmd.ExecuteNonQueryAsync();

            MessageBox.Show("Item updated successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            await Load();
            Clear();
        }

        // ===================== DELETE =====================

        private async Task Delete()
        {
            if (SelectedItem == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete \"{SelectedItem.ItemName}\"?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                "DELETE FROM Lost_Items WHERE LostItemID=@id", conn);

            cmd.Parameters.AddWithValue("@id", SelectedItem.LostItemID);

            await cmd.ExecuteNonQueryAsync();

            MessageBox.Show("Item deleted successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            await Load();
            Clear();
        }

        // ===================== CLEAR =====================

        private void Clear()
        {
            NewItem = new LostItem();
            SelectedItem = null;
        }

        // ===================== BACK =====================

        private void Back()
        {
            var dashboard = new AdminDashboard();
            dashboard.Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w is LostItems)
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