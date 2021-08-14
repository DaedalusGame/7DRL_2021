using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DRL_2021
{
    enum VictoryType
    {
        Win,
        Loss,
    }

    class GameOverType : RegistryEntry<GameOverType>
    {
        public string Reason;
        public VictoryType VictoryType;

        public GameOverType(string id, string reason, VictoryType victoryType) : base(id)
        {
            Reason = reason;
            VictoryType = victoryType;
        }

        public static GameOverType Death = new GameOverType("death", "GLORY TO THE BLOOD GOD.", VictoryType.Loss);

        public static GameOverType NemesisKill = new GameOverType("nemesis_kill", "BUT WILL MY NIGHTMARE END?", VictoryType.Win);
    }
}
