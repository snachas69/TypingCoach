using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TypingCoach.Service;

namespace TypingCoach.MVVM.ViewModel
{
    public class RegistrationViewModel : DependencyObject
    {
        private Action<string, string>? _action;
        private TextBox? _login;
        private PasswordBox? _password;
        public RegistrationViewModel(TextBox? login, PasswordBox? password, Action<string, string>? action)
        {
            _action = action;
            _login = login;
            _password = password;

            OKCommand = new RelayCommand(Execute_OK, CanExecute_OK);
            CloseCommand = new RelayCommand(Execute_Close);
        }

        //Commands
        public ICommand OKCommand { get; }
        private void Execute_OK(object? obj)
        {
            if (obj is Window window)
            {
                _action?.Invoke(_login?.Text ?? "", _password?.Password ?? "");
                window.Close();
            }
        }
        private bool CanExecute_OK(object? obj) 
            => _password?.Password.Count(i => i < 32 && i > 126) == 0 && _password.Password.Length > 6 && _login?.Text.Length >= 4;
        public ICommand CloseCommand { get; }
        private void Execute_Close(object? obj)
        {
            if (obj is Window window)
                window.Close();
        }

    }
}
