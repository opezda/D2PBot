namespace D2InfoBot.Parser.Structures {
    public enum ProfileАvailability {
        Aviable, Closed, DoesNotExists
    }
    public struct ProfileInfo {
        public ProfileАvailability Availability;
        public  string Name;
        public     int Wins;
        public     int Losses;
        public  double Winrate;
        public  string SkillBracket;
        public  string Rank;
        public  string RankImageUrl;
        public  string RankStarsImageUrl;
        public  string AvatarImageUrl;
        public  string Url;
        public  Hero[] Heroes;
        public Match[] Matches;
    }
}