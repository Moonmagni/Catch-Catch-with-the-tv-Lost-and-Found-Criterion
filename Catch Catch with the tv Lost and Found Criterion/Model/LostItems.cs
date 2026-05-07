using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Model
{
    public class LostItem : INotifyPropertyChanged
    {
        private int _lostItemID;
        public int LostItemID
        {
            get => _lostItemID;
            set { _lostItemID = value; OnPropertyChanged(); }
        }

        private string _itemName;
        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(); }
        }

        private string _itemDescription;
        public string ItemDescription
        {
            get => _itemDescription;
            set { _itemDescription = value; OnPropertyChanged(); }
        }

        private string _locationFound;
        public string LocationFound
        {
            get => _locationFound;
            set { _locationFound = value; OnPropertyChanged(); }
        }

        private DateTime _dateFound;
        public DateTime DateFound
        {
            get => _dateFound;
            set { _dateFound = value; OnPropertyChanged(); }
        }

        private bool _isFound;
        public bool IsFound
        {
            get => _isFound;
            set { _isFound = value; OnPropertyChanged(); }
        }

        private int _employeeID;
        public int EmployeeID
        {
            get => _employeeID;
            set { _employeeID = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}