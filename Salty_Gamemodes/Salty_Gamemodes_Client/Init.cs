﻿using CitizenFX.Core;
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

        Commands commands;
        Testing test;
        public SaltyTown Salty;

        Vector3 spawnPos = Vector3.Zero;
        public Dictionary<string, Map> Maps = new Dictionary<string, Map>();

        public float textTimer = 0f;
        public float textTime = 15 * 60 * 1000;

        public static int ScreenWidth = 0;
        public static int ScreenHeight = 0;

        VoteMenu voteMenu;


        public Init() {

            RequestStreamedTextureDict("saltyTextures", true);

            ClearAreaOfObjects( Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 100, 0 );

            commands = new Commands(this);
            //test = new Testing(this);
            Salty = new SaltyTown();

            textTimer = GetGameTimer() + textTime;


            EventHandlers[ "onClientResourceStart" ] += new Action<string>( OnClientResourceStart );
            EventHandlers[ "playerSpawned" ] += new Action<ExpandoObject>( PlayerSpawn );
            EventHandlers[ "baseevents:onPlayerDied" ] += new Action<int, List<dynamic>>( PlayerDied );
            EventHandlers[ "baseevents:onPlayerKilled" ] += new Action<int, ExpandoObject>( PlayerKilled );
            EventHandlers[ "salty::StartGame" ] += new Action<int, int, double, Vector3, Vector3, string, Vector3, ExpandoObject>( StartGame );
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
            EventHandlers[ "salty::SpawnWeapon" ] += new Action<string, uint, int, Vector3, bool, float, int, int>(SpawnWeapon);
            EventHandlers[ "salty::ClearWeapons" ] += new Action<string>(ClearWeapons);
            EventHandlers[ "salty::CreateMap" ] += new Action<string, Vector3, Vector3>(CreateMap);
            EventHandlers[ "salty::ShowRooms" ] += new Action<List<dynamic>>( ShowRoom );
            EventHandlers[ "salty::RoomLeft" ] += new Action( RoomLeft );

            Tick += Update;
            SetMaxWantedLevel( 0 );
            GetScreenActiveResolution( ref ScreenWidth, ref ScreenHeight );
        }

        public void SpawnWeapon( string wepModel, uint pickupHash, int worldHash, Vector3 gunPos, bool playerDropped, float waitTime, int ammoCount, int ammoInClip ) {
            if( Salty.ActiveGame != null )
                Salty.ActiveGame.GameMap.SpawnWeapon( wepModel, pickupHash, worldHash, gunPos, playerDropped, waitTime, ammoCount, ammoInClip );
        }

        public void ClearWeapons( string map ) {
            Debug.WriteLine( "Clearing weapons " + map );
            Maps[map].ClearWeapons();
        }

        public void ShowRoom( List<dynamic> rooms ) {
            List<string> names = new List<string>();
            foreach( string name in rooms ) {
                names.Add( name );
            }
            Salty.ShowRooms( names );
        }

        public void RoomLeft() {
            Salty.RoomLeft();
        }

        public void StartGame( int id, int team, double duration, Vector3 mapPos, Vector3 mapSize, string mapName, Vector3 startPos, ExpandoObject gunSpawns ) {
            GetScreenActiveResolution( ref ScreenWidth, ref ScreenHeight );
            if( voteMenu != null )
                voteMenu.Close();
            voteMenu = null;
            Map map = new Map( mapPos, mapSize, mapName );
            Salty.StartGame( id, team, duration, map, startPos, ExpandoToDictionary( gunSpawns ) );
        }

        public void CreateMap(string mapName, Vector3 position, Vector3 size) {
            Salty.ActiveMaps.Add( mapName, new Map( position, size, mapName ) );
            Salty.ActiveMaps[mapName].CreateBlip();
        }

        public void SpawnDeadBody( Vector3 position, int ply, int killer ) {
            if( Salty.ActiveGame is TTT ) {
                (Salty.ActiveGame as TTT).SpawnDeadBody( position, ply, killer );
            }
        }

        public void ViewDeadBody( int body ) {
            if( Salty.ActiveGame is TTT ) {
                (Salty.ActiveGame as TTT).BodyDiscovered( body );
            }
        }

        public void UpdateScore( int amount ) {
            Salty.ActiveGame.UpdateScore(amount);
        }

        public void UpdatePlayerInfo(int entID, string key, int value) {
            Debug.WriteLine( "Player info" );
            Salty.ActiveGame.UpdatePlayerInfo( GetPlayerFromServerId( entID ), key, value);
        }

        public void EndGame() {
            if( Maps.ContainsKey( Salty.ActiveGame.GameMap.Name ) ) {
                Maps[Salty.ActiveGame.GameMap.Name].ClearBlip();
                Maps.Remove( Salty.ActiveGame.GameMap.Name );
            }
            Salty.ActiveGame.End();
            Salty.ActiveGame = new BaseGamemode();
            if( !Salty.ActiveGame.isNoclip ) {
                Salty.ActiveGame.SetNoClip( true );
            }
        }
        public void UpdateGameTime( double time ) {
            Salty.ActiveGame.GameTime += time;
        }


        public void UpdateInfo( int id, double duration, Vector3 mapPos, Vector3 mapSize ) {
            Map map = new Map( mapPos, mapSize, "" );
            if( id == 1 ) { // Trouble in Terrorist Town
                Salty.ActiveGame = new TTT( map, 0 );            
            }
            if( id == 2 ) { // Drive or die
                Salty.ActiveGame = new DriveOrDie( map, 0 );

            }
            if( id == 3 ) { // Murder
                Salty.ActiveGame = new Murder( map, 0 );
            }
            if( id == 4 ) { // Murder
                Salty.ActiveGame = new IceCreamMan( map, 0 );
            }
            Salty.ActiveGame.inGame = true;
            if( duration > 0 )
                Salty.ActiveGame.CreateGameTimer( duration );
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
            if( Salty.isInRoom ) {
                Salty.ActiveGame.PlayerKilled( killerID, deathData );
            }
            else {
                Salty.SaltyGame.PlayerKilled( killerID, deathData );
            }
        }

        private void PlayerDied( int killerType, List<dynamic> deathcords ) {
            Vector3 coords = new Vector3( (float)deathcords[ 0 ], (float)deathcords[ 1 ], (float)deathcords[ 2 ] );
            if( Salty.isInRoom ) {
                Salty.ActiveGame.PlayerDied( killerType, coords );
            } else {
                Salty.SaltyGame.PlayerDied( killerType, coords );
            }
        }

        private void PlayerSpawn( ExpandoObject spawnInfo ) {

            if( spawnPos != Vector3.Zero ) {
                Game.Player.Character.Position = spawnPos;
                Salty.ActiveGame.SetNoClip( true );
                spawnPos = Vector3.Zero;
            } else if( Salty.isInRoom ) {
                Salty.ActiveGame.PlayerSpawned( spawnInfo );
            } else {
                Salty.SaltyGame.PlayerSpawned( spawnInfo );
            }

        }

        public async Task Update() {
            foreach( var map in Maps ) {
                if( map.Value.isVisible ) {
                    map.Value.DrawSpawnPoints();
                    map.Value.DrawBoundarys();
                }
            }

            foreach( var map in Salty.ActiveMaps ) {
                map.Value.DrawBoundarys();
            }

            if( test != null )
                test.Update();

            Salty.Update();

            if( textTimer - GetGameTimer() < 0 ) {
                Salty.SaltyGame.WriteChat( "SaltyTown", "/rooms to join a room and /startroom to start the room if it isn't started. /leaveroom to leave a room.", 255, 255, 255 );
                textTimer = GetGameTimer() + textTime;
            }

        }


        public void GiveGun( string weapon, int ammo ) {
            Salty.ActiveGame.GiveGun(weapon, ammo);
        }

        private void OnClientResourceStart( string resourceName ) {
            if( GetCurrentResourceName() != resourceName ) return;

            TriggerServerEvent( "salty::netJoined" );
            if( commands != null )
                commands.Load();
            if( test != null )
                test.LoadCommands();
            Salty.SaltyGame.WriteChat( "SaltyTown", "/rooms to join a room and /startroom to start the room if it isn't started. /leaveroom to leave a room.", 255, 255, 255 );

        }
    }
}
