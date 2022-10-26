using System;

namespace TypingCoach.MVVM.Model
{
    public class PlayerScoreModel
    {
        public PlayerScoreModel(string? player, DateTime sessionBeginning, TimeSpan duration, int fails, double speed, double complexity)
        {
            Player = player;
            SessionBeginning = sessionBeginning;
            Duration = duration;
            Fails = fails;
            Speed = speed;
            Complexity = complexity;
        }

        public string? Player { get; set; }
        public DateTime SessionBeginning { get; set; }
        public TimeSpan Duration { get; set; }
        public int Fails { get; set; }
        public double Speed { get; set; }
        public double Complexity { get; set; }

        public override string ToString() 
            => $"{Player}|{SessionBeginning}|{Duration}|{Fails}|{Speed}|{Complexity}\n";
    }
}
