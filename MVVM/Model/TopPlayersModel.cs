using System;

namespace TypingCoach.MVVM.Model
{
    public class TopPlayersModel
    {
        public TopPlayersModel(int place, string? player, double speedPlayer, TimeSpan durationPlayer, int fails, double complexity)
        {
            Place = place;
            Player = player;
            Speed = speedPlayer;
            Duration = durationPlayer;
            Fails = fails;
            Complexity = complexity;
        }

        public string? Player { get; set; }
        public TimeSpan Duration { get; set; }
        public double Speed { get; set; }
        public int Place { get; set; }
        public int Fails { get; set; }
        public double Complexity { get; set; }
    }
}
