using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TypingCoach.MVVM.Model;
using TypingCoach.Service;

namespace TypingCoach.MVVM.ViewModel
{
    public class MainViewModel : DependencyObject
    {
        public MainViewModel(Grid keyboard, TextBox textBox, ProgressBar progressBar)
        {
            foreach (StackPanel stackPanel in keyboard.Children)
                foreach (Button button in stackPanel.Children)
                    _keyboard?.Add(button.Name, button);

            _textBox = textBox;
            _progressBar = progressBar;
            _speedTracker.Tick += SpeedTracking;

            SettingsCommand = new RelayCommand(Execute_Settings, CanExecute_Settings);
            ScoreCommand = new RelayCommand(Execute_Score, CanExecute_Score);
            PlayCommand = new RelayCommand(Execute_Play);
            KeyPressCommand = new RelayCommand(Execute_KeyPress, CanExecute_Keys);
            KeyReleaseCommand = new RelayCommand(Execute_KeyRelease, CanExecute_Keys);
            ProgressbarValueChangedCommand = new RelayCommand(Execute_ProgressbarValueChanged);
        }

        //Fields
        private bool _isCapsLock;
        private Dictionary<string, Button?>? _keyboard = new();
        private TextBox? _textBox;
        private ProgressBar? _progressBar;
        private string? _text;
        private DispatcherTimer _speedTracker = new() { Interval = TimeSpan.FromMilliseconds(1000) };
        public DateTime _sessionBeginning;
        public double _complexity = Properties.Settings.Default.Complexity;

        //Dependency propreties
        public string? PlayOrStop
        {
            get { return (string?)GetValue(PlayOrStopProperty); }
            set { SetValue(PlayOrStopProperty, value); }
        }
        public static readonly DependencyProperty PlayOrStopProperty =
            DependencyProperty.Register("PlayOrStop", typeof(string), typeof(MainViewModel), new PropertyMetadata("Play"));
        public SolidColorBrush PlayBackground
        {
            get { return (SolidColorBrush)GetValue(PlayBackgroundProperty); }
            set { SetValue(PlayBackgroundProperty, value); }
        }
        public static readonly DependencyProperty PlayBackgroundProperty =
            DependencyProperty.Register("PlayBackground", typeof(SolidColorBrush), typeof(MainViewModel), new PropertyMetadata(Brushes.Lime));

        public bool IsGameStarted
        {
            get { return (bool)GetValue(IsGameStartedProperty); }
            set { SetValue(IsGameStartedProperty, value); }
        }
        public static readonly DependencyProperty IsGameStartedProperty =
            DependencyProperty.Register("IsGameStarted", typeof(bool), typeof(MainViewModel), new PropertyMetadata(false));

        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(double), typeof(MainViewModel), new PropertyMetadata(0.0));

        public int Fails
        {
            get { return (int)GetValue(FailsProperty); }
            set { SetValue(FailsProperty, value); }
        }
        public static readonly DependencyProperty FailsProperty =
            DependencyProperty.Register("Fails", typeof(int), typeof(MainViewModel), new PropertyMetadata(0));

        public string? Player
        {
            get { return (string)GetValue(PlayerProperty); }
            set { SetValue(PlayerProperty, value); }
        }
        public static readonly DependencyProperty PlayerProperty =
            DependencyProperty.Register("Player", typeof(string), typeof(MainViewModel), new PropertyMetadata(Properties.Settings.Default.PlayerName));

        //Commands
        public ICommand SettingsCommand { get; }
        private bool CanExecute_Settings(object? obj) => !IsGameStarted;
        private void Execute_Settings(object? obj)
        {
            new Settings().ShowDialog();
            Player = Properties.Settings.Default.PlayerName;
            _complexity = Properties.Settings.Default.Complexity;
        }

        public ICommand ScoreCommand { get; }
        private bool CanExecute_Score(object? obj) => !IsGameStarted && Player != "Guest";
        private void Execute_Score(object? obj) => new Score().ShowDialog();

        public ICommand PlayCommand { get; }
        private void Execute_Play(object? obj)
        {
            if (!IsGameStarted)
            {
                IsGameStarted = true;
                _isCapsLock = (Keyboard.GetKeyStates(Key.Capital) & KeyStates.Toggled) == KeyStates.Toggled;
                if (_isCapsLock) LettersToUpper();
                IsGameStarted = true;
                Fails = 0;
                Speed = 0.0;
                _text = string.Empty;
                _textBox.Text = DataRecorder.GetText(_complexity);
                _progressBar.Maximum = _textBox.Text.Length;
                _progressBar.Value = 0;
                PlayOrStop = "Stop";
                PlayBackground = Brushes.Red;
                _sessionBeginning = DateTime.Now;
                _speedTracker.Start();
            }
            else Stop();
        }


        public ICommand KeyPressCommand { get; }
        private void Execute_KeyPress(object? obj)
        {
            if (obj is KeyEventArgs e && _keyboard.ContainsKey(e.Key.ToString()))
            {
                _keyboard[e.Key.ToString()].ClickMode = ClickMode.Press;

                switch (e.Key)
                {
                    case Key.LeftCtrl:
                    case Key.RightCtrl:
                    case Key.LWin:
                    case Key.RWin:
                    case Key.RightAlt:
                    case Key.LeftAlt:
                    case Key.Back:
                        return;
                    case Key.LeftShift:
                    case Key.RightShift:
                        Shift();
                        return;
                    case Key.Capital:
                        CapsLock();
                        return;
                    case Key.Space:
                        Type(' ');
                        return;
                    case Key.Tab:
                        Type('\t');
                        return;
                    case Key.Enter:
                        Type('\n');
                        return;
                    default:
                        Type(char.Parse(_keyboard[e.Key.ToString()]?.Content as string ?? " "));
                        return;
                }
            }
        }
        private bool CanExecute_Keys(object? obj) => IsGameStarted;
        public ICommand KeyReleaseCommand { get; }
        private void Execute_KeyRelease(object? obj)
        {
            if (obj is KeyEventArgs e && _keyboard.ContainsKey(e.Key.ToString()))
            {
                _keyboard[e.Key.ToString()].ClickMode = ClickMode.Release;

                if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                {
                    if (_isCapsLock) LettersToUpper();
                    else LettersToLower();
                    NonLettersToLower();
                }
            }
        }

        public ICommand ProgressbarValueChangedCommand { get; }

        private void Execute_ProgressbarValueChanged(object? obj)
        {
            if (_progressBar?.Value == _progressBar?.Maximum)
            {
                Stop();
                TimeSpan duration = _sessionBeginning - DateTime.Now;
                MessageBox.Show(
                    $"Fails: {Fails}\nSpeed: {Speed}",
                    "Game over!", MessageBoxButton.OK, MessageBoxImage.Information
                    );

                if (Player != "Guest") 
                    DataRecorder.WriteResaults(
                        new PlayerScoreModel(Player, _sessionBeginning, duration, Fails, Speed, _complexity)
                        );
                _progressBar.Value = 0;
            }
        }

        //Regular methods
        private void LettersToUpper()
        {
            _keyboard["A"].Content = "A";
            _keyboard["B"].Content = "B";
            _keyboard["C"].Content = "C";
            _keyboard["D"].Content = "D";
            _keyboard["E"].Content = "E";
            _keyboard["F"].Content = "F";
            _keyboard["G"].Content = "G";
            _keyboard["H"].Content = "H";
            _keyboard["I"].Content = "I";
            _keyboard["J"].Content = "J";
            _keyboard["K"].Content = "K";
            _keyboard["L"].Content = "L";
            _keyboard["M"].Content = "M";
            _keyboard["N"].Content = "N";
            _keyboard["O"].Content = "O";
            _keyboard["P"].Content = "P";
            _keyboard["Q"].Content = "Q";
            _keyboard["R"].Content = "R";
            _keyboard["S"].Content = "S";
            _keyboard["T"].Content = "T";
            _keyboard["U"].Content = "U";
            _keyboard["V"].Content = "V";
            _keyboard["W"].Content = "W";
            _keyboard["X"].Content = "X";
            _keyboard["Y"].Content = "Y";
            _keyboard["Z"].Content = "Z";
        }
        private void LettersToLower()
        {
            _keyboard["A"].Content = "a";
            _keyboard["B"].Content = "b";
            _keyboard["C"].Content = "c";
            _keyboard["D"].Content = "d";
            _keyboard["E"].Content = "e";
            _keyboard["F"].Content = "f";
            _keyboard["G"].Content = "g";
            _keyboard["H"].Content = "h";
            _keyboard["I"].Content = "i";
            _keyboard["J"].Content = "j";
            _keyboard["K"].Content = "k";
            _keyboard["L"].Content = "l";
            _keyboard["M"].Content = "m";
            _keyboard["N"].Content = "n";
            _keyboard["O"].Content = "o";
            _keyboard["P"].Content = "p";
            _keyboard["Q"].Content = "q";
            _keyboard["R"].Content = "r";
            _keyboard["S"].Content = "s";
            _keyboard["T"].Content = "t";
            _keyboard["U"].Content = "u";
            _keyboard["V"].Content = "v";
            _keyboard["W"].Content = "w";
            _keyboard["X"].Content = "x";
            _keyboard["Y"].Content = "y";
            _keyboard["Z"].Content = "z";
        }
        private void NonLettersToUpper()
        {
            _keyboard["Oem3"].Content = "~";
            _keyboard["D1"].Content = "!";
            _keyboard["D2"].Content = "@";
            _keyboard["D3"].Content = "#";
            _keyboard["D4"].Content = "$";
            _keyboard["D5"].Content = "%";
            _keyboard["D6"].Content = "^";
            _keyboard["D7"].Content = "&";
            _keyboard["D8"].Content = "*";
            _keyboard["D9"].Content = "(";
            _keyboard["D0"].Content = ")";
            _keyboard["OemMinus"].Content = "_";
            _keyboard["OemPlus"].Content = "+";
            _keyboard["OemOpenBrackets"].Content = "{";
            _keyboard["Oem6"].Content = "}";
            _keyboard["Oem5"].Content = "|";
            _keyboard["Oem1"].Content = ":";
            _keyboard["OemQuotes"].Content = "\"";
            _keyboard["OemComma"].Content = "<";
            _keyboard["OemPeriod"].Content = ">";
            _keyboard["OemQuestion"].Content = "?";

        }
        private void NonLettersToLower()
        {
            _keyboard["Oem3"].Content = "`";
            _keyboard["D1"].Content = "1";
            _keyboard["D2"].Content = "2";
            _keyboard["D3"].Content = "3";
            _keyboard["D4"].Content = "4";
            _keyboard["D5"].Content = "5";
            _keyboard["D6"].Content = "6";
            _keyboard["D7"].Content = "7";
            _keyboard["D8"].Content = "8";
            _keyboard["D9"].Content = "9";
            _keyboard["D0"].Content = "0";
            _keyboard["OemMinus"].Content = "-";
            _keyboard["OemPlus"].Content = "=";
            _keyboard["OemOpenBrackets"].Content = "[";
            _keyboard["Oem6"].Content = "]";
            _keyboard["Oem5"].Content = "\\";
            _keyboard["Oem1"].Content = ";";
            _keyboard["OemQuotes"].Content = "'";
            _keyboard["OemComma"].Content = ",";
            _keyboard["OemPeriod"].Content = ".";
            _keyboard["OemQuestion"].Content = "/";
        }
        private void Type(char character)
        {
            if (_textBox?.Text[0] != character)
            {
                Fails++;
                _textBox.Foreground = Brushes.Red;
                return;
            }

            _text += character;
            _textBox.Foreground = Brushes.White;
            _textBox.Text = _textBox.Text.Remove(0, 1);
            _progressBar.Value++;
        }
        private void Shift()
        {
            if (_isCapsLock) LettersToLower();
            else LettersToUpper();
            NonLettersToUpper();
        }
        private void CapsLock()
        {
            if (_isCapsLock) LettersToLower();
            else LettersToUpper();
            _isCapsLock = !_isCapsLock;
        }
        private void Stop()
        {
            _speedTracker.Stop();
            IsGameStarted = false;
            PlayOrStop = "Play";
            PlayBackground = Brushes.Lime;
        }
        private void SpeedTracking(object? sender, EventArgs e) =>
            Speed = 60 * Math.Round(_text.Length / (DateTime.Now - _sessionBeginning).TotalSeconds);
        
    }
}
