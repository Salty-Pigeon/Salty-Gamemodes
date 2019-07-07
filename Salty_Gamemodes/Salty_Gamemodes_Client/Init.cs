using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using CitizenFX.Core.Native;

namespace Salty_Gamemodes_Client
{
    public class Init : BaseScript
    {
        public BaseGamemode ActiveGame = new BaseGamemode();
        Commands commands;
        Testing test;

        Vector3 spawnPos = Vector3.Zero;
        public Dictionary<string, Map> Maps = new Dictionary<string, Map>();

        public static int ScreenWidth = 0;
        public static int ScreenHeight = 0;

        VoteMenu voteMenu;


        public Init() {

            RequestStreamedTextureDict("saltyTextures", true);

            ClearAreaOfObjects( Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 100, 0 );

            //commands = new Commands(this);
            //test = new Testing(this);

            EventHandlers[ "onClientResourceStart" ] += new Action<string>( OnClientResourceStart );
            EventHandlers[ "playerSpawned" ] += new Action<ExpandoObject>( PlayerSpawn );
            EventHandlers[ "baseevents:onPlayerDied" ] += new Action<int, List<dynamic>>( PlayerDied );
            EventHandlers[ "baseevents:onPlayerKilled" ] += new Action<int, ExpandoObject>( PlayerKilled );
            EventHandlers[ "salty::StartGame" ] += new Action<int, int, double, Vector3, Vector3, Vector3, ExpandoObject>( StartGame );
            EventHandlers[ "salty::EndGame" ] += new Action( EndGame );
            EventHandlers[ "salty::SpawnPointGUI" ] += new Action<ExpandoObject, ExpandoObject>( SpawnGUI );
            EventHandlers[ "salty::VoteMap" ] += new Action<List<dynamic>>( VoteMap );
            EventHandlers[ "salty::GiveGun" ] += new Action<string, int>( GiveGun );
            EventHandlers[ "salty::UpdateScore" ] += new Action<int>( UpdateScore );
            EventHandlers[ "salty::UpdateInfo" ] += new Action<int, double, Vector3, Vector3>( UpdateInfo );
            EventHandlers[ "salty::GMPlayerUpdate" ] += new Action<int, string, int>(UpdatePlayerInfo);
            EventHandlers[ "salty::SpawnDeadBody" ] += new Action<Vector3, int, int>(SpawnDeadBody);
            EventHandlers[ "salty::UpdateDeadBody" ] += new Action<int>(ViewDeadBody);
            EventHandlers[ "salty::UpdateGameTime" ] += new Action<double>(UpdateGameTime);
            ActiveGame.SetTeam( 0 );
            ActiveGame.SetNoClip( true );
            Tick += Update;
            SetMaxWantedLevel( 0 );
            GetScreenActiveResolution( ref ScreenWidth, ref ScreenHeight );
        }

        public void StartGame( int id, int team, double duration, Vector3 mapPos, Vector3 mapSize, Vector3 startPos, ExpandoObject gunSpawns ) {
            NetworkSetVoiceChannel( 0 );
            GetScreenActiveResolution( ref ScreenWidth, ref ScreenHeight );
            voteMenu.Close();
            voteMenu = null;
            if( ActiveGame.inGame )
                ActiveGame.End();
            ActiveGame.SetNoClip( false );
            Map map = new Map( mapPos, mapSize, "" );
            map.GunSpawns = ExpandoToDictionary( gunSpawns );
            if( id == 1 ) { // Trouble in Terrorist Town
                ActiveGame = new TTT(map, team);
            }
            if( id == 2 ) { // Drive or die
                ActiveGame = new DriveOrDie( map, team );
            }
            if( id == 3 ) { // Murder
                ActiveGame = new Murder( map, team );
            }
            if( id == 4 ) { // Ice Cream Man
                ActiveGame = new IceCreamMan( map, team );
            }
            if( duration > 0 )
                ActiveGame.CreateGameTimer( duration );
            ActiveGame.PlayerSpawn = startPos;
            Game.Player.Character.Position = startPos;
            ActiveGame.Start();
        }

        public void SpawnDeadBody( Vector3 position, int ply, int killer ) {
            if( ActiveGame is TTT ) {
                (ActiveGame as TTT).SpawnDeadBody( position, ply, killer );
            }
        }

        public void ViewDeadBody( int body ) {
            if( ActiveGame is TTT ) {
                (ActiveGame as TTT).BodyDiscovered( body );
            }
        }

        public void UpdateScore( int amount ) {
            ActiveGame.UpdateScore(amount);
        }

        public void UpdatePlayerInfo(int entID, string key, int value) {
            ActiveGame.UpdatePlayerInfo( GetPlayerFromServerId( entID ), key, value);
        }

        public void EndGame() {
            ActiveGame.End();
            ActiveGame = new BaseGamemode();
            if( !ActiveGame.isNoclip ) {
                ActiveGame.noclipPos = Game.PlayerPed.Position;
                ActiveGame.SetNoClip( true );
            }
        }
        public void UpdateGameTime( double time ) {
            ActiveGame.GameTime += time;
        }


        public void UpdateInfo( int id, double duration, Vector3 mapPos, Vector3 mapSize ) {
            Map map = new Map( mapPos, mapSize, "" );
            if( id == 1 ) { // Trouble in Terrorist Town
                ActiveGame = new TTT( map, 0 );            
            }
            if( id == 2 ) { // Drive or die
                ActiveGame = new DriveOrDie( map, 0 );

            }
            if( id == 3 ) { // Murder
                ActiveGame = new Murder( map, 0 );
            }
            if( id == 4 ) { // Murder
                ActiveGame = new IceCreamMan( map, 0 );
            }
            ActiveGame.inGame = true;
            if( duration > 0 )
                ActiveGame.CreateGameTimer( duration );
            Game.Player.Character.Position = mapPos + new Vector3( 0, 0, 50 );
            spawnPos = mapPos + new Vector3( 0, 0, 50 );
        }

        public Dictionary<string,List<Vector3>> ExpandoToDictionary( ExpandoObject keyValuePairs ) {
            Dictionary<string, List<Vector3>> spawns = new Dictionary<string, List<Vector3>>();
            foreach( var spawn in keyValuePairs ) {

                List<Vector3> spawnPoints = new List<Vector3>();
                List<object> conversion = spawn.Value as List<object>;
                foreach( Vector3 vec in conversion ) {
                    spawnPoints.Add( vec );
                }
                spawns.Add( spawn.Key.ToString(), spawnPoints );
            }
            return spawns;
        }
        private void VoteMap( List<dynamic> maps ) {
            List<string> mapsList = maps.OfType<string>().ToList();
            if( voteMenu == null ) {
                voteMenu = new VoteMenu( this, "Vote Gamemode", "Vote for the next gamemode", mapsList );
            } else {
                voteMenu.Close();
                voteMenu = new VoteMenu( this, "Vote Map", "Vote for next map", mapsList );
            }
        }


        private void SpawnGUI( ExpandoObject mapObj, ExpandoObject spawnObj ) {

            Dictionary<string, Dictionary<int, List<Vector3>>> spawns = new Dictionary<string, Dictionary<int, List<Vector3>>>();
            
            foreach( var map in spawnObj ) {
                spawns.Add( map.Key, new Dictionary<int, List<Vector3>>() );
                foreach( var mapSpawns in map.Value as ExpandoObject ) {
                    spawns[map.Key].Add( Convert.ToInt32(mapSpawns.Key), new List<Vector3>() );
                    foreach( Vector3 spawn in mapSpawns.Value as List<object> ) {
                        spawns[map.Key][Convert.ToInt32(mapSpawns.Key)].Add( spawn );
                    }
                }
            }

            Dictionary<string, Map> Maps = new Dictionary<string, Map>();

            foreach( var obj in mapObj ) {
                List<Vector3> bounds = new List<Vector3>();
                List<object> conversion = obj.Value as List<object>;
                foreach( Vector3 vec in conversion ) {
                    bounds.Add( vec );
                }

                if( Maps.ContainsKey(obj.Key) ) {
                    Maps[obj.Key].Position = bounds[0];
                    Maps[obj.Key].Size = bounds[1];
                    Maps[obj.Key].SpawnPoints = spawns[obj.Key];
                }
                else {
                    Map map = new Map( bounds[0], bounds[1], obj.Key );
                    map.SpawnPoints = spawns[obj.Key];
                    Maps.Add( obj.Key, map );
                }

            }

            this.Maps = new Dictionary<string, Map>();

            foreach( var map in Maps ) {
                if( this.Maps.ContainsKey(map.Key) ) {
                    this.Maps.Add( map.Key, map.Value );
                }
            }

            this.Maps = Maps;

            MapMenu menu = new MapMenu( this, "Maps", "Modify maps", this.Maps );
        }

        private void PlayerKilled( int killerID, ExpandoObject deathData  ) {

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
            PlayerDied( killerType, deathCoords );
            ActiveGame.PlayerKilled( killerID, deathData );
        }

        private void PlayerDied( int killerType, List<dynamic> deathcords ) {
            Vector3 coords = new Vector3( (float)deathcords[ 0 ], (float)deathcords[ 1 ], (float)deathcords[ 2 ] );
            ActiveGame.PlayerDied( killerType, coords );
        }

        private void PlayerSpawn( ExpandoObject spawnInfo ) {

            if( spawnPos != Vector3.Zero ) {
                Game.Player.Character.Position = spawnPos;
                ActiveGame.noclipPos = spawnPos;
                ActiveGame.SetNoClip( true );
                spawnPos = Vector3.Zero;
            } else {
                ActiveGame.PlayerSpawned( spawnInfo );
            }

        }

        public async Task Update() {
            if( ActiveGame != null )
                ActiveGame.Update();
            foreach( var map in Maps ) {
                if( map.Value.isVisible ) {
                    map.Value.DrawSpawnPoints();
                    map.Value.DrawBoundarys();
                }
            }

            if( test != null )
                test.Update();

        }


        public void GiveGun( string weapon, int ammo ) {
            ActiveGame.GiveGun(weapon, ammo);
        }

        private void OnClientResourceStart( string resourceName ) {
            if( GetCurrentResourceName() != resourceName ) return;

            ActiveGame.SetNoClip( false );

            TriggerServerEvent( "salty::netJoined" );
            if( commands != null )
                commands.Load();
            if( test != null )
                test.LoadCommands();

        }
    }
}
