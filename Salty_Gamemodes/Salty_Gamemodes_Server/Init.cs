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
            EventHandlers[ "salty::netModifyMapPos" ] += new Action<Player, string, string, Vector3>( ModifyMapPosition );
            EventHandlers[ "salty::netModifyMap" ] += new Action<Player, string, string, Vector3, Vector3>( ModifyMap );

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

        private void ModifyMap([FromSource] Player ply, string setting, string mapName, Vector3 position, Vector3 size ) {
            if( setting == "delete" ) {
                Debug.WriteLine( "Deleteing " + mapName );
                MapManager.Maps.Remove( mapName );
                SQLConnection.Remove( mapName );
            }
            if( setting == "add" ) {
                Debug.WriteLine( "Creating " + mapName );
                if( MapManager.Maps.ContainsKey( mapName ) )
                    return;
                Map map = new Map( position, size, mapName );
                MapManager.Maps.Add( mapName, map );
                MapManager.Maps[mapName].AddSpawnPoint( position );
                SQLConnection.SaveMap( MapManager.Maps[ mapName ] );
            }
        }

        private void ModifyMapPosition([FromSource] Player ply, string setting, string mapName, Vector3 pos ) {
            if( setting == "delete" ) {
                Debug.WriteLine( "Deleteing " + pos.ToString() + " from map " + mapName );
                MapManager.Maps[ mapName ].SpawnPoints.Remove( pos );
                SQLConnection.SaveMap( MapManager.Maps[mapName] );
            }
            if( setting == "add" ) {
                if( mapName == "AUTO" ) {
                    mapName = MapManager.FindInsideMap( pos ).Name;
                }
                Debug.WriteLine( "Adding " + pos.ToString() + " to map " + mapName );
                MapManager.Maps[ mapName ].SpawnPoints.Add( pos );
                SQLConnection.SaveMap( MapManager.Maps[ mapName ] );
            }
        }

        private void SpawnPointGUI([FromSource] Player ply) {
            // ply.TriggerEvent( "salty::SpawnPointGUI", MapManager.AllMapsSpawns(), MapManager.AllMapsSizes() );
             ply.TriggerEvent( "salty::SpawnPointGUI", MapManager.AllMapsBounds(), MapManager.AllMapsSpawns() );
        }

    }
}
