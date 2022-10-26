using System;
using System.Windows;
using TypingCoach.MVVM.ViewModel;

namespace TypingCoach
{
    public partial class Registration : Window
    {
        public Registration(Action<string, string> action)
        {
            InitializeComponent();
            DataContext = new RegistrationViewModel(login, password, action);
        }
    }
}
