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
            TTT,
            DriveOrDie,
            Murder
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
            EventHandlers[ "salty::netModifyMapPos" ] += new Action<Player, string, string, int, Vector3>( ModifyMapPosition );
            EventHandlers[ "salty::netModifyWeaponPos" ] += new Action<Player, string, string, string, Vector3>( ModifyWeaponPosition );
            EventHandlers[ "salty::netModifyMap" ] += new Action<Player, string, string, int, Vector3, Vector3>( ModifyMap );

            SQLConnection = new Database();
            MapManager = new MapManager( SQLConnection.Load() );

            RegisterCommand( "startTTT", new Action<int, List<object>, string>( ( source, args, raw ) => {
                StartTTT();
            } ), false );

            RegisterCommand( "startMurder", new Action<int, List<object>, string>( ( source, args, raw ) => {
                StartMurder();
            } ), false );

            RegisterCommand( "endGame", new Action<int, List<object>, string>( ( source, args, raw ) => {
                EndGame();
            } ), false );

            RegisterCommand( "loadSQL", new Action<int, List<object>, string>( ( source, args, raw ) => {
                SQLConnection.Load();
            } ), false );

            RegisterCommand( "saveSQL", new Action<int, List<object>, string>( ( source, args, raw ) => {
                SQLConnection.SaveAll(MapManager.Maps);
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

        public void StartMurder() {
            Gamemode = Gamemodes.Murder;

            Random rand = new Random();
            List<Map> maps = MapManager.MapList( "mmm" );
            Map map = maps[rand.Next( 0, maps.Count )];

            PlayerList players = new PlayerList();

            ActiveGame = new Murder( map, players, (int)Gamemodes.Murder );
            ActiveGame.Start();

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

        private void ModifyMap([FromSource] Player ply, string setting, string mapName, int team, Vector3 position, Vector3 size ) {
            Debug.WriteLine( string.Format( "{0} {1} {2} {3} {4}", setting, mapName, team.ToString(), position.ToString(), size.ToString() ) );
            if( setting == "delete" ) {
                MapManager.Maps.Remove( mapName );
                SQLConnection.Remove( mapName );
            }
            if( setting == "add" ) {
                if( MapManager.Maps.ContainsKey( mapName ) )
                    return;
                Map map = new Map( position, size, mapName );
                MapManager.Maps.Add( mapName, map );
                MapManager.Maps[mapName].AddSpawnPoint( team, position );
                SQLConnection.SaveMap( MapManager.Maps[ mapName ] );
            }
            if( setting == "edit" ) {
                if( !MapManager.Maps.ContainsKey( mapName ) )
                    return;
                MapManager.Maps[mapName].Position = position;
                MapManager.Maps[mapName].Size = size;
                SQLConnection.SaveMap( MapManager.Maps[mapName] );
            }
        }

        private void ModifyWeaponPosition( [FromSource] Player ply, string setting, string mapName, string weapon, Vector3 position ) {
            if( setting == "delete" ) {
                MapManager.Maps[mapName].DeleteWeaponSpawn( weapon, position );
                SQLConnection.SaveMap( MapManager.Maps[mapName] );
            }
            if( setting == "add" ) {
                if( mapName == "AUTO" ) {
                    mapName = MapManager.FindInsideMap( position ).Name;
                    MapManager.Maps[mapName].AddWeaponSpawn( weapon, position );
                    SQLConnection.SaveMap( MapManager.Maps[mapName] );
                }
            }
        }

        private void ModifyMapPosition([FromSource] Player ply, string setting, string mapName, int team, Vector3 pos ) {
            if( setting == "delete" ) {
                MapManager.Maps[ mapName ].DeleteSpawnPoint( team, pos );
                SQLConnection.SaveMap( MapManager.Maps[mapName] );
            }
            if( setting == "add" ) {
                if( mapName == "AUTO" ) {
                    mapName = MapManager.FindInsideMap( pos ).Name;
                }
                MapManager.Maps[mapName].AddSpawnPoint( team, pos );
                SQLConnection.SaveMap( MapManager.Maps[ mapName ] );
            }
        }

        private void SpawnPointGUI([FromSource] Player ply) {
            // ply.TriggerEvent( "salty::SpawnPointGUI", MapManager.AllMapsSpawns(), MapManager.AllMapsSizes() );
             ply.TriggerEvent( "salty::SpawnPointGUI", MapManager.AllMapsBounds(), MapManager.AllMapsSpawns() );
        }

    }
}
