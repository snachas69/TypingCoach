using System;
using System.IO;
using System.Linq;
using TypingCoach.MVVM.Model;

namespace TypingCoach.Service
{
    public static class DataRecorder
    {
        public static void WriteAccount(string? login, string? password)
        {
            string path = Directory.GetCurrentDirectory() + "\\Accounts.txt";
            if (!File.Exists(path)) File.Create(path);
            File.AppendAllText(path, $"{login}-{password}\n");
            Properties.Settings.Default.PlayerName = login;
            Properties.Settings.Default.Save();
        }

        public static void WriteResaults(PlayerScoreModel player)
        {
            string path = Directory.GetCurrentDirectory() + "\\Scores.txt";
            if (!File.Exists(path)) 
                File.Create(path).Close();
            File.AppendAllText(path, player.ToString());
        }

        public static void ThrowAnExceptionIfAccountHasNotBeenFound(string? login, string? password)
        {
            string path = Directory.GetCurrentDirectory() + "\\Accounts.txt";
            if (!File.Exists(path)) 
                File.Create(path).Close();
            if (File.ReadAllLines(path).Where(i => i == $"{login}-{password}").Count() == 0)
                throw new Exception("Wrong login or password");
            Properties.Settings.Default.PlayerName = login;
            Properties.Settings.Default.Save();
        }

        public static TopPlayersModel[] GetTop()
            => GetScores()
                ?.OrderByDescending(i => i.Complexity)
                ?.ThenByDescending(i => i.Speed)
                ?.ThenBy(i => i.Fails)
                ?.Select((e, i) => new TopPlayersModel(i + 1, e.Player, e.Speed, e.Duration, e.Fails, e.Complexity))
                ?.ToArray();


        public static PlayerScoreModel[] GetAccountScore(string? login) 
            => GetScores()
                 .Where(i => i.Player == login)
                 .ToArray();
        private static PlayerScoreModel[] GetScores() 
            => File.ReadAllLines(Directory.GetCurrentDirectory() + "\\Scores.txt")
                   .Select(i =>
                   {
                       string[] s = i.Split('|', StringSplitOptions.RemoveEmptyEntries);
                       return new PlayerScoreModel(s[0], DateTime.Parse(s[1]), TimeSpan.Parse(s[2]), int.Parse(s[3]), double.Parse(s[4]), double.Parse(s[5]));
                   })
                   .ToArray();

        public static string GetText(double complexity) 
            => File.ReadAllText(Directory.GetCurrentDirectory() + $"\\Texts\\Complexity {complexity}\\Text {new Random().Next(1, 11)}.txt");
    }
}
