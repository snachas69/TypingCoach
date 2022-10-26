using System.Windows;
using TypingCoach.MVVM.ViewModel;

namespace TypingCoach
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }
    }
}
