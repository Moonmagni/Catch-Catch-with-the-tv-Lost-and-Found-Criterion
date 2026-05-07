using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Core;
using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Model;
using Catch_Catch_with_the_tv_Lost_and_Found_Criterion.View;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.ViewModel
{
    public class LoginViewModel : ObservableObject
    {
        private readonly string connectionString =
            @"Server=THOMAS;Database=Catch Catch with the TV;Trusted_Connection=True;TrustServerCertificate=True;";

        public UserModel CurrentUser { get; set; } = new();

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(async p => await Login(p));
        }

        private async Task Login(object param)
        {
            var pb = param as PasswordBox;
            if (pb == null) return;

            string password = pb.Password;

            if (string.IsNullOrWhiteSpace(CurrentUser.Username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using SqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                // FIXED: column is UserID not EmployeeID
                var cmd = new SqlCommand(
                    "SELECT UserID, Role FROM Users WHERE Username=@u AND Password=@p", conn);

                cmd.Parameters.AddWithValue("@u", CurrentUser.Username.Trim());
                cmd.Parameters.AddWithValue("@p", password);

                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // FIXED: read UserID not EmployeeID
                    SharedData.CurrentUserID = (int)reader["UserID"];
                    string role = reader["Role"].ToString();
                    await reader.CloseAsync();

                    // All roles go to AdminDashboard for now
                    new AdminDashboard().Show();

                    foreach (Window w in Application.Current.Windows)
                    {
                        if (w is AdminLogin)
                        {
                            w.Close();
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}