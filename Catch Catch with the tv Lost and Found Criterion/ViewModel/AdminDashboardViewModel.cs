using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Core;
using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.View;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.ViewModel
{
    public class AdminDashboardViewModel : INotifyPropertyChanged
    {
        public ICommand ManageLostItemsCommand { get; }
        public ICommand ViewClaimsCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminDashboardViewModel()
        {
            ManageLostItemsCommand = new RelayCommand(_ => OpenAndClose(new LostItems()));
            ViewClaimsCommand = new RelayCommand(_ => OpenAndClose(new Claims()));
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        private void OpenAndClose(Window next)
        {
            next.Show();
            CloseCurrentWindow();
        }

        private void Logout()
        {
            new AdminLogin().Show();
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w is AdminDashboard)
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