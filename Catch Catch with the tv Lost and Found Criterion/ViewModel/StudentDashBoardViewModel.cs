using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Core;
using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.View;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.ViewModel
{
    public class StudentDashboardViewModel : INotifyPropertyChanged
    {
        public ICommand ViewLostItemsCommand { get; }
        public ICommand LogoutCommand { get; }

        public StudentDashboardViewModel()
        {
            ViewLostItemsCommand = new RelayCommand(_ => OpenLostItems());
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        private void OpenLostItems()
        {
            new StudentLostItems().Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w is StudentDashboard)
                {
                    w.Close();
                    break;
                }
            }
        }

        private void Logout()
        {
            new MainDashboard().Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w is StudentDashboard)
                {
                    w.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}