using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Model
{
    public class Claim : INotifyPropertyChanged
    {
        private int _claimID;
        public int ClaimID
        {
            get => _claimID;
            set { _claimID = value; OnPropertyChanged(); }
        }

        private int _lostItemID;
        public int LostItemID
        {
            get => _lostItemID;
            set { _lostItemID = value; OnPropertyChanged(); }
        }

        private int _studentID;
        public int StudentID
        {
            get => _studentID;
            set { _studentID = value; OnPropertyChanged(); }
        }

        private int? _employeeID;
        public int? EmployeeID
        {
            get => _employeeID;
            set { _employeeID = value; OnPropertyChanged(); }
        }

        private string _verificationStatus;
        public string VerificationStatus
        {
            get => _verificationStatus;
            set { _verificationStatus = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}