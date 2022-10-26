using System;
using System.Windows;
using System.Windows.Input;
using TypingCoach.Service;

namespace TypingCoach.MVVM.ViewModel
{
    public class SettingsViewModel : DependencyObject
    {
        //Dependency properties
        public double Complexity
        {
            get { return (double)GetValue(ComplexityProperty); }
            set { SetValue(ComplexityProperty, value); }
        }
        public static readonly DependencyProperty ComplexityProperty =
            DependencyProperty.Register("Complexity", typeof(double), typeof(SettingsViewModel), 
                new PropertyMetadata(Properties.Settings.Default.Complexity));

        //Commands
        public ICommand LogInCommand { get; }
        private void Execute_LogIn(object? obj) => new Registration(DataRecorder.ThrowAnExceptionIfAccountHasNotBeenFound).ShowDialog();

        public ICommand SignUpCommand { get; }
        private void Execute_SignUp(object? obj)
        {
            try 
            { new Registration(DataRecorder.WriteAccount).ShowDialog(); }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "The player has not been found",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ICommand LogOutCommand { get; }
        private void Execute_LogOut(object? obj)
        {
            if (Properties.Settings.Default.PlayerName != "Guest")
            {
                MessageBox.Show($"{Properties.Settings.Default.PlayerName}, you have successfuly quit your account", "Quit",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Properties.Settings.Default.PlayerName = "Guest";
            }
        }

        public ICommand SaveCommand { get; }
        private void Execute_Save(object? obj)
        {
            if (obj is Window window)
            {
                Properties.Settings.Default.Complexity = Complexity;
                MessageBox.Show("Settings were saved", "Saved!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Properties.Settings.Default.Save();
                window.Close();
            }
        }

        public ICommand CloseCommand { get; }
        private void Execute_Close(object? obj)
        {
            if (obj is Window window)
                window.Close();
        }

        public SettingsViewModel()
        {
            LogInCommand = new RelayCommand(Execute_LogIn);
            SignUpCommand = new RelayCommand(Execute_SignUp);
            LogOutCommand = new RelayCommand(Execute_LogOut);
            SaveCommand = new RelayCommand(Execute_Save);
            CloseCommand = new RelayCommand(Execute_Close);
        }

    }
}
