using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server
{
    public class Init : BaseScript {

        enum Gamemodes {
            None,
            TTT
        }

        public bool inGame = false;
        BaseGamemode ActiveGame = new BaseGamemode( (int)Gamemodes.None );

        Database SQLConnection;
        MapManager MapManager;

        Gamemodes Gamemode = Gamemodes.None;

        public Init() {
            EventHandlers[ "salty::netStartGame" ] += new Action( ActiveGame.Start );
            EventHandlers[ "salty::netEndGame" ] += new Action( ActiveGame.End );
            EventHandlers[ "salty::netSpawnPointGUI" ] += new Action<Player>( SpawnPointGUI );

            SQLConnection = new Database();
            MapManager = new MapManager( SQLConnection.Load() );

            RegisterCommand( "startTTT", new Action<int, List<object>, string>( ( source, args, raw ) => {
                StartTTT();
            } ), false );

            RegisterCommand( "endGame", new Action<int, List<object>, string>( ( source, args, raw ) => {
                EndGame();
            } ), false );


            Tick += Init_Tick;
        }

        private async Task Init_Tick() {
            if( ActiveGame != null )
                ActiveGame.Update();
        }

        public void EndGame() {
            ActiveGame.End();
            Gamemode = Gamemodes.None;
            ActiveGame = new BaseGamemode( (int)Gamemodes.None );
        }

        public void StartTTT() {
            Gamemode = Gamemodes.TTT;

            Random rand = new Random();
            List<Map> maps = MapManager.MapList( "ttt" );
            Map map = maps[ rand.Next( 0, maps.Count ) ];

            PlayerList players = new PlayerList();
            
            ActiveGame = new TTT( map, players, (int)Gamemodes.TTT );
            ActiveGame.Start();
           
        }

        private void SpawnPointGUI([FromSource] Player ply) {
            ply.TriggerEvent( "salty::SpawnPointGUI", MapManager.AllMapsSpawns() );
        }

    }
}
