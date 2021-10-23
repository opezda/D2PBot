using System.Collections.Generic;

namespace D2InfoBot {
    public static class Data{
        public static int MmrByRank(string rank){
            List<string> ranks = new List<string>(new [] { "Herald", "Guardian", "Crusader", "Archon", "Legend", "Ancient", "Divine" });
            List<string> stars = new List<string>(new [] { "I", "II", "III", "IV", "V" });
            string[] temp = rank.Replace("Rank: ", "").Split(" ");
            return (ranks.IndexOf(temp[0]) * 5 + stars.IndexOf(temp[1]) + 2) * 155;
        }
    }
}