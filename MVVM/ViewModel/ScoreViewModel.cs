using System.Collections.ObjectModel;
using TypingCoach.MVVM.Model;
using TypingCoach.Service;

namespace TypingCoach.MVVM.ViewModel
{
    public class ScoreViewModel
    {
        public ObservableCollection<TopPlayersModel>? TopPlayers { get; set; }
        public ObservableCollection<PlayerScoreModel>? PlayerScore { get; set; }

        public ScoreViewModel()
        {
            TopPlayers = new ObservableCollection<TopPlayersModel>(DataRecorder.GetTop());
            PlayerScore = new ObservableCollection<PlayerScoreModel>(DataRecorder.GetAccountScore(Properties.Settings.Default.PlayerName));
        }
    }
}
