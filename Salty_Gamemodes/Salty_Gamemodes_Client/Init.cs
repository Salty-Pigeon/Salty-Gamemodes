﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Salty_Gamemodes_Client
{
    public class Init : BaseScript
    {
        BaseGamemode ActiveGame = new BaseGamemode();

        public Dictionary<string, Map> Maps = new Dictionary<string, Map>();

        public Init() {
            EventHandlers[ "onClientResourceStart" ] += new Action<string>( OnClientResourceStart );
            EventHandlers[ "playerSpawned" ] += new Action<ExpandoObject>( PlayerSpawn );
            EventHandlers[ "baseevents:onPlayerDied" ] += new Action<int, List<dynamic>>( PlayerDied );
            EventHandlers[ "salty::StartGame" ] += new Action<int, int, Vector3, Vector3, Vector3>( StartGame );
            EventHandlers[ "salty::EndGame" ] += new Action( ActiveGame.End );
            EventHandlers[ "salty::CreateMap" ] += new Action( ActiveGame.CreateMap );
            EventHandlers[ "salty::SpawnPointGUI" ] += new Action<ExpandoObject, ExpandoObject>( SpawnGUI );
            ActiveGame.SetNoClip( false );
            Tick += Update;
        }

        public void StartGame( int id, int team, Vector3 mapPos, Vector3 mapSize, Vector3 startPos ) {
            Map map = new Map( mapPos, mapSize, "" );
            if( id == 1 ) { // Trouble in Terrorist Town
                ActiveGame = new TTT(map, team);
                if( team == 1 ) { // Traitor
                    
                } else if( team == 2 ) { // Innocent

                } else { // Spectator

                }
            }
            ActiveGame.Start();
            Game.Player.Character.Position = startPos;
        }

        private void SpawnGUI( ExpandoObject mapObj, ExpandoObject spawnObj ) {


            Dictionary<string, List<Vector3>> spawns = new Dictionary<string, List<Vector3>>();
            foreach( var spawn in spawnObj ) {
                List<Vector3> spawnPoints = new List<Vector3>();
                List<object> conversion = spawn.Value as List<object>;
                foreach( Vector3 vec in conversion ) {
                    spawnPoints.Add( vec );
                }
                spawns.Add( spawn.Key.ToString(), spawnPoints );
            }

            foreach( var obj in mapObj ) {
                List<Vector3> bounds = new List<Vector3>();
                List<object> conversion = obj.Value as List<object>;
                foreach( Vector3 vec in conversion ) {
                    bounds.Add( vec );
                }
                Debug.WriteLine( obj.Key );

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

            MapMenu menu = new MapMenu( this, "Maps", "Modify maps", Maps );
        }

        private void PlayerDied( int killerType, List<dynamic> deathcords ) {
            Vector3 coords = new Vector3( (float)deathcords[ 0 ], (float)deathcords[ 1 ], (float)deathcords[ 2 ] );
            ActiveGame.PlayerDied( killerType, coords );
        }

        private void PlayerSpawn( ExpandoObject spawnInfo ) {
            ActiveGame.PlayerSpawned( spawnInfo );
        }

        public async Task Update() {
            if( ActiveGame != null )
                ActiveGame.Update();
        }

        private void OnClientResourceStart( string resourceName ) {
            if( GetCurrentResourceName() != resourceName ) return;

            ActiveGame.SetNoClip( false );

            RegisterCommand( "noclip", new Action<int, List<object>, string>( ( source, args, raw ) => {
                ActiveGame.SetNoClip(!ActiveGame.isNoclip);
            } ), false );

            RegisterCommand( "spawnPoints", new Action<int, List<object>, string>( ( source, args, raw ) => {
                TriggerServerEvent( "salty::netSpawnPointGUI" );
            } ), false );

            RegisterCommand( "addspawnpoint", new Action<int, List<object>, string>( ( source, args, raw ) => {
                TriggerServerEvent( "salty::netModifyMapPos", "add", "AUTO", Game.Player.Character.Position );
            } ), false );

            RegisterCommand( "createmap", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( args[ 2 ] == null )
                    return;
                TriggerServerEvent( "salty::netModifyMap", "add", args[0], Game.Player.Character.Position, new Vector3( float.Parse( args[1].ToString() ), float.Parse( args[2].ToString() ), 0 ) );
            } ), false );

        }
    }
}
