using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Core
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}