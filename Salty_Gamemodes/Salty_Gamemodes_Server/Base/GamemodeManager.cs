using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    public class GamemodeManager : BaseScript {

        public enum Gamemodes {
            None,
            TTT,
            DriveOrDie,
            Murder,
            IceCreamMan
        }

        public BaseGamemode StartGame( int gamemode, Map map, List<Player> players ) {
            BaseGamemode Game;
            switch( (Gamemodes)gamemode ) {
                case Gamemodes.TTT:
                    Game = new TTT( (int)Gamemodes.TTT, map, players );
                    Game.CreateGameTimer( 10 * 60 );
                    Game.Start();
                    break;
                case Gamemodes.Murder:
                    Game = new Murder( (int)Gamemodes.Murder, map, players );
                    Game.CreateGameTimer( 5 * 60 );
                    Game.Start();
                    break;
                case Gamemodes.IceCreamMan:
                    Game = new IceCreamMan( (int)Gamemodes.IceCreamMan, map, players );
                    Game.CreateGameTimer( 8 * 60 );
                    Game.Start();
                    break;
                case Gamemodes.DriveOrDie:
                    Game = new DriveOrDie( (int)Gamemodes.DriveOrDie, map, players );
                    Game.CreateGameTimer( 15 * 60 );
                    Game.Start();
                    break;
                default:
                    Game = new BaseGamemode( (int)Gamemodes.None, null, players );
                    Game.CreateGameTimer( 15 * 60 );
                    Game.Start();
                    break;
            }
            return Game;
        }
    }
}
