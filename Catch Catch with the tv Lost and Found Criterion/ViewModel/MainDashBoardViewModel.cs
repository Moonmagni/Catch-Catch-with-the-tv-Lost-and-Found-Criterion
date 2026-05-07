using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Core;
using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.View;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.ViewModel
{
    public class MainDashboardViewModel : INotifyPropertyChanged
    {
        public ICommand StudentCommand { get; }
        public ICommand AdminCommand { get; }

        public MainDashboardViewModel()
        {
            StudentCommand = new RelayCommand(_ => OpenStudent());
            AdminCommand = new RelayCommand(_ => OpenAdmin());
        }

        private void OpenStudent()
        {
            new StudentLogin().Show();
            CloseCurrentWindow();
        }

        private void OpenAdmin()
        {
            new AdminLogin().Show();
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
            foreach (Window w in Application.Current.Windows)
            {
                if (w is MainDashboard)
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