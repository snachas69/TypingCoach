using System.Windows;
using TypingCoach.MVVM.ViewModel;

namespace TypingCoach
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(keyboard, textBox, proceedingProgressBar);
        }
    }
}
