using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    public class SaltyTown : BaseScript {

        Salty MainGame;
        MapManager Maps;
        GamemodeManager GamemodeManager;
        List<Player> ActivePlayers = new List<Player>();
        Dictionary<int, BaseGamemode> ActiveGamemodes = new Dictionary<int, BaseGamemode>();
        Dictionary<int, List<Player>> ActiveRooms = new Dictionary<int, List<Player>>();

        Dictionary<GamemodeManager.Gamemodes, string> GamemodeMaps = new Dictionary<GamemodeManager.Gamemodes, string>() {
            { GamemodeManager.Gamemodes.TTT, "ttt" },
            { GamemodeManager.Gamemodes.Murder, "mmm" },
            { GamemodeManager.Gamemodes.IceCreamMan, "icm" },
            { GamemodeManager.Gamemodes.DriveOrDie, "dod" },
        };

        public SaltyTown( MapManager maps ) {
            Maps = maps;
            MainGame = new Salty();
            GamemodeManager = new GamemodeManager();
            ActivePlayers = new PlayerList().ToList();
        }

        public void JoinRoom( Player ply, int gamemode ) { 
            if( !ActivePlayers.Contains( ply ) ) {
                Debug.WriteLine( ply.Name + " is already in game" );
                return;
            }
            if( !ActiveRooms.ContainsKey(gamemode) ) {
                Debug.WriteLine( "No gamemode created, creating." );
                StartRoom( gamemode );
            }
            ActiveRooms[gamemode].Add( ply );

        }

        public void StartRoom( int gamemode ) {
            if( ActiveRooms.ContainsKey(gamemode) ) {
                Debug.WriteLine( "Room already created for: " + (GamemodeManager.Gamemodes)gamemode );
                return;
            }
            ActiveRooms.Add( gamemode, new List<Player>() );
        }

        public Map RandomMap( string mapTag ) {
            Random rand = new Random();
            List<Map> maps = Maps.MapList( mapTag );
            return maps[rand.Next( 0, maps.Count )];
        }

        public void StartGame( int gamemode, Map map ) {
            if( !ActiveRooms.ContainsKey(gamemode ) ) {
                Debug.WriteLine( "No room created" );
            }
            if( ActiveGamemodes.ContainsKey(gamemode) ) {
                Debug.WriteLine( "Game in progress." );
            } else {
                ActiveGamemodes.Add( gamemode, GamemodeManager.StartGame( gamemode, map, ActiveRooms[gamemode] ) );
            }
        }

        public void StartGame( int gamemode ) {
            if( !GamemodeMaps.ContainsKey((GamemodeManager.Gamemodes)gamemode) ) {
                Debug.WriteLine( "No gamemode found" );
                return;
            }
            if( !ActiveRooms.ContainsKey( gamemode ) ) {
                Debug.WriteLine( "No room created" );
                return;
            }
            if( ActiveGamemodes.ContainsKey( gamemode ) ) {
                Debug.WriteLine( "Game in progress." );
                return;
            }
            else {
                Map map = RandomMap( GamemodeMaps[(GamemodeManager.Gamemodes)gamemode] );
                ActiveGamemodes.Add( gamemode, GamemodeManager.StartGame( gamemode, map, ActiveRooms[gamemode] ) );
                map.SendToClients();
            }
        }

        public void EndGame( int gamemode ) {
            ActiveGamemodes[gamemode].End();
            ActiveGamemodes.Remove( gamemode );
        }

        public void LeaveRoom() {

        }




    }
}
