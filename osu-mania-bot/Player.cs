using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amatsu
{
    class Player
    {
        public List<string> Scoreslist;
        
        public Player(string username, List<string> scores)
        {
            Scoreslist = scores;
        }
    }
}
