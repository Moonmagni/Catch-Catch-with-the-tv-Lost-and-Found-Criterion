using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.View
{
    /// <summary>
    /// Interaction logic for StudentClaim.xaml
    /// </summary>
    public partial class StudentClaim : Window
    {
        public StudentClaim()
        {
            InitializeComponent();
            DataContext = new ViewModel.StudentClaimViewModel();
        }
    }
}
