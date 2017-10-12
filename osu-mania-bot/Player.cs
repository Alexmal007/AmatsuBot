using System.Collections.Generic;

namespace Amatsu
{
    class Player
    {
        public double SuccessRateMod = 0;
        public List<string> Scoreslist;
        public List<string> DTList = new List<string>();
        
        public Player(string username, List<string> scores)
        {
            Scoreslist = scores;
        }
    }
}
