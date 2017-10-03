using System.Collections.Generic;

namespace Amatsu
{
    class Player
    {
        public double SuccessRateMod = 0;
        public List<string> Scoreslist;
        
        public Player(string username, List<string> scores)
        {
            Scoreslist = scores;
        }
    }
}
