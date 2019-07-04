using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Salty_Gamemodes_Server
{
    public class Init : BaseScript {

        enum Gamemodes {
            None,
            TTT,
            DriveOrDie,
            Murder,
            IceCreamMan
        }


        public bool inGame = false;
        public static BaseGamemode ActiveGame;

        Database SQLConnection;
        MapManager MapManager;

        public Init() {

            SQLConnection = new Database();
            MapManager = new MapManager( SQLConnection.Load() );
            ActiveGame = new BaseGamemode( MapManager, (int)Gamemodes.None, "*" );


            EventHandlers["playerDropped"] += new Action<Player, string>(ActiveGame.PlayerDropped);

            EventHandlers["baseevents:onPlayerDied"] += new Action<Player, int, List<dynamic>>(PlayerDied);
            EventHandlers["baseevents:onPlayerKilled"] += new Action<Player, int, ExpandoObject>(PlayerKilled);

            EventHandlers[ "salty::netStartGame" ] += new Action( ActiveGame.Start );
            EventHandlers[ "salty::netEndGame" ] += new Action( EndGame );
            EventHandlers[ "salty::netSpawnPointGUI" ] += new Action<Player>( SpawnPointGUI );
            EventHandlers[ "salty::netModifyMapPos" ] += new Action<Player, string, string, int, Vector3>( ModifyMapPosition );
            EventHandlers[ "salty::netModifyWeaponPos" ] += new Action<Player, string, string, string, Vector3>( ModifyWeaponPosition );
            EventHandlers[ "salty::netModifyMap" ] += new Action<Player, string, string, int, Vector3, Vector3>( ModifyMap );
            EventHandlers[ "salty::netAddScore" ] += new Action<Player, int>( AddScoreToPlayer );
            EventHandlers[ "salty::netVoteMap" ] += new Action<Player, string>( MapManager.PlayerVote );
            EventHandlers[ "salty::netJoined" ] += new Action<Player>( PlayerJoined );
            EventHandlers[ "salty::netUpdatePlayerBool" ] += new Action<Player, string>( UpdatePlayerBool );


            RegisterCommand( "startTTT", new Action<int, List<object>, string>( ( source, args, raw ) => {
                StartTTT();
            } ), false );

            RegisterCommand( "startMurder", new Action<int, List<object>, string>( ( source, args, raw ) => {
                StartMurder();
            } ), false );

            RegisterCommand( "startDod", new Action<int, List<object>, string>( ( source, args, raw ) => {
                StartDriveOrDie();
            } ), false );

            RegisterCommand( "startIceCreamMan", new Action<int, List<object>, string>( ( source, args, raw ) => {
                StartIceCreamMan();
            } ), false );


            RegisterCommand( "endGame", new Action<int, List<object>, string>( ( source, args, raw ) => {
                EndGame();
            } ), false );

            RegisterCommand( "voteMap", new Action<int, List<object>, string>( ( source, args, raw ) => {
                MapManager.VoteMapGUI( "ttt" );
            } ), false );

            RegisterCommand( "loadSQL", new Action<int, List<object>, string>( ( source, args, raw ) => {
                SQLConnection.Load();
            } ), false );

            RegisterCommand( "saveSQL", new Action<int, List<object>, string>( ( source, args, raw ) => {
                SQLConnection.SaveAll(MapManager.Maps);
            } ), false );

            Tick += Init_Tick;
        }

        public void PlayerJoined( [FromSource]Player ply ) {

            if( ActiveGame.ID != (int)Gamemodes.None ) {
                ply.TriggerEvent( "salty::UpdateInfo", ActiveGame.ID, ActiveGame.GameTime - GetGameTimer(), ActiveGame.GameMap.Position, ActiveGame.GameMap.Size );
            }
            ActiveGame.PlayerJoined( ply );
        }

        public void UpdatePlayerBool( [FromSource]Player ply, string key ) {
            ActiveGame.UpdatePlayerBoolean(ply, key);
        }

        private async Task Init_Tick() {
            if( ActiveGame != null )
                ActiveGame.Update();
            MapManager.Update();
        }

        public void EndGame() {
            ActiveGame.End();
        }

        public void StartIceCreamMan() {
            ActiveGame = new IceCreamMan( MapManager, (int)Gamemodes.IceCreamMan, "icm" );
            ActiveGame.CreateGameTimer( 3 * 60 );
            ActiveGame.Start();
        }

        public void StartMurder() {
            ActiveGame = new Murder( MapManager, (int)Gamemodes.Murder, "mmm" );
            ActiveGame.CreateGameTimer( 3 * 60 );
            ActiveGame.Start();
        }

        public void StartDriveOrDie() {
            ActiveGame = new DriveOrDie( MapManager, (int)Gamemodes.DriveOrDie, "dod" );
            ActiveGame.CreateGameTimer( 3 * 60 );
            ActiveGame.Start();
        }

        public void StartTTT() {
            ActiveGame = new TTT( MapManager, (int)Gamemodes.TTT, "ttt" );
            ActiveGame.CreateGameTimer( 3 * 60 );
            ActiveGame.Start();
        }

        private void PlayerKilled( [FromSource] Player ply, int killerID, ExpandoObject deathData ) {
            int killerType = 0;
            List<dynamic> deathCoords = new List<dynamic>();
            foreach( var data in deathData ) {
                if( data.Key == "killertype" ) {
                    killerType = (int)data.Value;
                }
                if( data.Key == "killerpos" ) {
                    deathCoords = data.Value as List<dynamic>;
                }
            }
            PlayerDied( ply, killerType, deathCoords );
            ActiveGame.PlayerKilled( ply, killerID, deathData );
        }

        private void PlayerDied( [FromSource] Player ply, int killerType, List<dynamic> deathcords ) {
            Vector3 coords = new Vector3( (float)deathcords[0], (float)deathcords[1], (float)deathcords[2] );
            ActiveGame.PlayerDied( ply, killerType, coords );
        }

        private void AddScoreToPlayer( [FromSource] Player ply, int amount ) {
            ActiveGame.AddScore( ply, amount );
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
             ply.TriggerEvent( "salty::SpawnPointGUI", MapManager.AllMapsBounds(), MapManager.AllMapsSpawns() );
        }

    }
}
