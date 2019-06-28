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
        BaseGamemode ActiveGame = new BaseGamemode();

        public Dictionary<string, Map> Maps = new Dictionary<string, Map>();

        public Init() {
            EventHandlers[ "onClientResourceStart" ] += new Action<string>( OnClientResourceStart );
            EventHandlers[ "playerSpawned" ] += new Action<ExpandoObject>( PlayerSpawn );
            EventHandlers[ "baseevents:onPlayerDied" ] += new Action<int, List<dynamic>>( PlayerDied );
            EventHandlers[ "salty::StartGame" ] += new Action<int, int, Vector3, Vector3, Vector3, ExpandoObject>( StartGame );
            EventHandlers[ "salty::EndGame" ] += new Action( ActiveGame.End );
            EventHandlers[ "salty::CreateMap" ] += new Action( ActiveGame.CreateMap );
            EventHandlers[ "salty::SpawnPointGUI" ] += new Action<ExpandoObject, ExpandoObject>( SpawnGUI );
            ActiveGame.SetNoClip( false );
            Tick += Update;
            SetMaxWantedLevel( 0 );       
        }

        public void StartGame( int id, int team, Vector3 mapPos, Vector3 mapSize, Vector3 startPos, ExpandoObject gunSpawns ) {
            if( ActiveGame.inGame )
                ActiveGame.End();
            ActiveGame.SetNoClip( false );
            Debug.WriteLine( "Starting game" );
            Map map = new Map( mapPos, mapSize, "" );
            map.GunSpawns = ExpandoToDictionary( gunSpawns );
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


        private void SpawnGUI( ExpandoObject mapObj, ExpandoObject spawnObj ) {


            Dictionary<string, List<Vector3>> spawns = ExpandoToDictionary( spawnObj );

            Dictionary<string, Map> Maps = new Dictionary<string, Map>();

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

            this.Maps = new Dictionary<string, Map>();

            foreach( var map in Maps ) {
                if( this.Maps.ContainsKey(map.Key) ) {
                    this.Maps.Add( map.Key, map.Value );
                }
            }

            this.Maps = Maps;

            MapMenu menu = new MapMenu( this, "Maps", "Modify maps", this.Maps );
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
            foreach( var map in Maps ) {
                if( map.Value.isVisible ) {
                    map.Value.DrawSpawnPoints();
                }
            }

            Weapon w = Game.PlayerPed?.Weapons?.Current;

            if (w != null) {
                WeaponHash wHash = w.Hash;

                if (API.GetHashKey(wHash.ToString()) != -1783943904) // add MarksmanRifle MKII
                {
                    API.HideHudComponentThisFrame(14);
                }
            }

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

            RegisterCommand( "addweaponpoint", new Action<int, List<object>, string>( ( source, args, raw ) => {
                TriggerServerEvent( "salty::netModifyWeaponPos", "add", "AUTO", args[0], Game.Player.Character.Position );
            } ), false );

            RegisterCommand( "createmap", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( args[ 2 ] == null )
                    return;
                TriggerServerEvent( "salty::netModifyMap", "add", args[0], Game.Player.Character.Position, new Vector3( float.Parse( args[1].ToString() ), float.Parse( args[2].ToString() ), 0 ) );
            } ), false );

            RegisterCommand( "test", new Action<int, List<object>, string>( ( source, args, raw ) => {
                //CreateObject( GetHashKey( "W_AR_ASSAULTRIFLE" ), Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z + 1, true, true, true );
                CreatePickup( (uint)GetHashKey( "PICKUP_WEAPON_SMG" ), Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z + 1, 0, 0, true, (uint)GetHashKey( "W_SB_SMG" ) );

            } ), false );




            RegisterCommand( "weapon", new Action<int, List<object>, string>( ( source, args, raw ) => {
                GiveWeaponToPed( Game.PlayerPed.Handle, (uint)GetHashKey( args[0].ToString() ), 999, false, true );
            } ), false );

            RegisterCommand( "car", new Action<int, List<object>, string>( async ( source, args, raw ) => {
                // account for the argument not being passed
                var model = "adder";
                if( args.Count > 0 ) {
                    model = args[0].ToString();
                }

                // check if the model actually exists
                // assumes the directive `using static CitizenFX.Core.Native.API;`
                var hash = (uint)GetHashKey( model );
                if( !IsModelInCdimage( hash ) || !IsModelAVehicle( hash ) ) {
                    TriggerEvent( "chat:addMessage", new {
                        color = new[] { 255, 0, 0 },
                        args = new[] { "[CarSpawner]", $"It might have been a good thing that you tried to spawn a {model}. Who even wants their spawning to actually ^*succeed?" }
                    } );
                    return;
                }

                // create the vehicle
                var vehicle = await World.CreateVehicle( model, Game.PlayerPed.Position, Game.PlayerPed.Heading );

                // set the player ped into the vehicle and driver seat
                Game.PlayerPed.SetIntoVehicle( vehicle, VehicleSeat.Driver );

                // tell the player
                TriggerEvent( "chat:addMessage", new {
                    color = new[] { 255, 0, 0 },
                    args = new[] { "[CarSpawner]", $"Woohoo! Enjoy your new ^*{model}!" }
                } );
            } ), false );

        }
    }
}
