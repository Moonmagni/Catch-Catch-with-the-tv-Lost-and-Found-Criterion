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
    public class StudentLoginViewModel : ObservableObject
    {
        private readonly string connectionString =
            @"Server=THOMAS;Database=Catch Catch with the TV;Trusted_Connection=True;TrustServerCertificate=True;";

        public UserModel CurrentUser { get; set; } = new();

        public ICommand LoginCommand { get; }
        public ICommand BackCommand { get; }

        public StudentLoginViewModel()
        {
            LoginCommand = new RelayCommand(async p => await Login(p));
            BackCommand = new RelayCommand(_ => Back());
        }

        private async Task Login(object param)
        {
            var pb = param as PasswordBox;
            if (pb == null) return;

            string password = pb.Password;

            if (string.IsNullOrWhiteSpace(CurrentUser.Username) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using SqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                var cmd = new SqlCommand(@"
                    SELECT UserID, Role, StudentID
                    FROM Users
                    WHERE Username = @u AND Password = @p", conn);

                cmd.Parameters.AddWithValue("@u", CurrentUser.Username.Trim());
                cmd.Parameters.AddWithValue("@p", password);

                var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    SharedData.CurrentUserID = (int)reader["UserID"];
                    string role = reader["Role"].ToString();

                    SharedData.CurrentStudentID =
                        reader["StudentID"] == DBNull.Value
                            ? 0
                            : (int)reader["StudentID"];

                    await reader.CloseAsync();

                    if (role == "Student")
                    {
                        if (SharedData.CurrentStudentID == 0)
                        {
                            MessageBox.Show(
                                "Your account has no student profile linked. Contact the administrator.",
                                "No Student Profile",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        new StudentDashboard().Show();

                        foreach (Window w in Application.Current.Windows)
                        {
                            if (w is StudentLogin)
                            {
                                w.Close();
                                break;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("This login is for students only.", "Access Denied",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void Back()
        {
            new MainDashboard().Show();

            foreach (Window w in Application.Current.Windows)
            {
                if (w is StudentLogin)
                {
                    w.Close();
                    break;
                }
            }
        }
    }
}