using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PU_Test.Model
{
    internal class GameInfo
    {

        public GameInfo(string gameExePath)
        {
            GameExePath = gameExePath;
        }

        public enum GameType
        {
            CN,
            OS,
            UnKnown,
        }
        public GameType GetGameType()
        {
            GameType gameType = GameType.UnKnown;

            if (File.Exists(Path.Combine(GameExePath, "YuanShen_Data")))
            {
                gameType = GameType.CN;
            }
            else if (File.Exists(Path.Combine(GameExePath, "GenshinImpact_Data")))
            {
                gameType = GameType.OS;
            }

            return gameType;
        }

        public string GameExePath { get; set; }

    }
}
