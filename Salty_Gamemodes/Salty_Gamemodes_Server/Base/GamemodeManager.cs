using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    public class GamemodeManager : BaseScript {

        public Dictionary<int, List<Player>> InGamePlayers = new Dictionary<int, List<Player>>();

        public enum Gamemodes {
            None,
            TTT,
            DriveOrDie,
            Murder,
            IceCreamMan
        }

        public Dictionary<string, int> Names = new Dictionary<string, int> {
            { "Trouble in Terrorist Town", (int)Gamemodes.TTT },
            { "Murder", (int)Gamemodes.Murder },
            { "Ice Cream Man", (int)Gamemodes.IceCreamMan },
            { "Drive Or Die", (int)Gamemodes.DriveOrDie },
        };

        public Dictionary<Gamemodes, string> Maps = new Dictionary<GamemodeManager.Gamemodes, string>() {
            { Gamemodes.TTT, "ttt" },
            { Gamemodes.Murder, "mmm" },
            { Gamemodes.IceCreamMan, "icm" },
            { Gamemodes.DriveOrDie, "dod" },
        };

        public List<string> GetNames() {
            return Names.OrderBy( x => x.Value ).ToDictionary( x => x.Key, x => x.Value ).Keys.ToList();
        }

        public void RemovePlayer( Player ply, int gamemode ) {
            if( InGamePlayers.ContainsKey(gamemode) ) {
                InGamePlayers[gamemode].Remove( ply );
            }
        }

        public void AddPlayer( Player ply, int gamemode ) {
            if( InGamePlayers.ContainsKey( gamemode ) ) {
                InGamePlayers[gamemode].Add( ply );
            } 
        }

        public int PlayerInGame( Player ply ) {
            foreach( var room in InGamePlayers ) {
                if( room.Value.Contains(ply) ) {
                    return room.Key;
                }
            }
            return 0;
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
            InGamePlayers[gamemode] = players;
            return Game;
        }
    }
}
