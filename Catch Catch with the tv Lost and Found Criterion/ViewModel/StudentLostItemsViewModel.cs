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
    public class StudentLostItemsViewModel : INotifyPropertyChanged
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

        public ICommand AddCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand BackCommand { get; }

        // ===================== CONSTRUCTOR =====================

        public StudentLostItemsViewModel()
        {
            AddCommand = new RelayCommand(async _ => await Add());
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

            var cmd = new SqlCommand(
                "SELECT * FROM Lost_Items ORDER BY LostItemID DESC", conn);
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

            FilteredItems = temp;
        }

        // ===================== ADD =====================

        private async Task Add()
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
                    (ItemName, ItemDescription, LocationFound, DateFound, IsFound)
                VALUES
                    (@name, @desc, @loc, @date, @found)", conn);

            cmd.Parameters.AddWithValue("@name", NewItem.ItemName ?? "");
            cmd.Parameters.AddWithValue("@desc", NewItem.ItemDescription ?? "");
            cmd.Parameters.AddWithValue("@loc", NewItem.LocationFound ?? "");
            cmd.Parameters.AddWithValue("@date", DateTime.Now);
            cmd.Parameters.AddWithValue("@found", false);

            await cmd.ExecuteNonQueryAsync();

            MessageBox.Show("Lost item reported successfully!", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            await Load();
            Clear();
        }

        // ===================== CLEAR =====================

        private void Clear()
        {
            NewItem = new LostItem();
        }

        // ===================== BACK =====================

        private void Back()
        {
            new StudentDashboard().Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w is StudentLostItems)
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