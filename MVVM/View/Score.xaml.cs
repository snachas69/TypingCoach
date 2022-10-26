using System.Windows;
using TypingCoach.MVVM.ViewModel;

namespace TypingCoach
{
    public partial class Score : Window
    {
        public Score()
        {
            InitializeComponent();
            DataContext = new ScoreViewModel();
        }
    }
}
