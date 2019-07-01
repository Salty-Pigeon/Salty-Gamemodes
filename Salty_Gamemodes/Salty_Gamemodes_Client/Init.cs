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
        Vector3 spawnPos = Vector3.Zero;
        public Dictionary<string, Map> Maps = new Dictionary<string, Map>();

        public Init() {
            EventHandlers[ "onClientResourceStart" ] += new Action<string>( OnClientResourceStart );
            EventHandlers[ "playerSpawned" ] += new Action<ExpandoObject>( PlayerSpawn );
            EventHandlers[ "baseevents:onPlayerDied" ] += new Action<int, List<dynamic>>( PlayerDied );
            EventHandlers[ "baseevents:onPlayerKilled" ] += new Action<int, ExpandoObject>( PlayerKilled );
            EventHandlers[ "salty::StartGame" ] += new Action<int, int, double, Vector3, Vector3, Vector3, ExpandoObject>( StartGame );
            EventHandlers[ "salty::EndGame" ] += new Action( EndGame );
            EventHandlers[ "salty::SpawnPointGUI" ] += new Action<ExpandoObject, ExpandoObject>( SpawnGUI );
            EventHandlers[ "salty::VoteMap" ] += new Action<List<dynamic>>( VoteMap );
            EventHandlers[ "salty::GiveGun" ] += new Action<string, int>( GiveGun );
            EventHandlers[ "salty::UpdateInfo" ] += new Action<int, double, Vector3, Vector3>( UpdateInfo );

            ActiveGame.SetNoClip( false );
            Tick += Update;
            SetMaxWantedLevel( 0 );


        }

        public void StartGame( int id, int team, double duration, Vector3 mapPos, Vector3 mapSize, Vector3 startPos, ExpandoObject gunSpawns ) {
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

        public void EndGame() {
            ActiveGame.End();
            ActiveGame = new BaseGamemode();
            ActiveGame.SetNoClip( true );
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

            VoteMenu menu = new VoteMenu( this, "Vote Map", "Vote for next map", mapsList );

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

        }


        public void GiveGun( string weapon, int ammo ) {
            GiveWeaponToPed( PlayerPedId(), (uint)GetHashKey( weapon ), ammo, false, true );
            SetPedAmmo( PlayerPedId(), (uint)GetHashKey( weapon ), ammo );
        }

        private void OnClientResourceStart( string resourceName ) {
            if( GetCurrentResourceName() != resourceName ) return;

            ActiveGame.SetNoClip( false );

            TriggerServerEvent( "salty::netJoined" );



            RegisterCommand( "noclip", new Action<int, List<object>, string>( ( source, args, raw ) => {
                ActiveGame.SetNoClip(!ActiveGame.isNoclip);
            } ), false );

            RegisterCommand( "mouse", new Action<int, List<object>, string>( ( source, args, raw ) => {
                Debug.WriteLine( string.Format( "{0} {1} {2}", GetGameplayCamRot( 0 ).X, GetGameplayCamRot( 0 ).Y, GetGameplayCamRot( 0 ).Z ) );
            } ), false );

            RegisterCommand( "respawn", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( ActiveGame is IceCreamMan ) {
                    (ActiveGame as IceCreamMan).Respawn();
                }
            } ), false );

            RegisterCommand( "heading", new Action<int, List<object>, string>( ( source, args, raw ) => {
                Debug.WriteLine( Game.PlayerPed.Heading.ToString() );
            } ), false );

            RegisterCommand( "spawnPoints", new Action<int, List<object>, string>( ( source, args, raw ) => {
                TriggerServerEvent( "salty::netSpawnPointGUI" );
            } ), false );

            RegisterCommand( "addspawnpoint", new Action<int, List<object>, string>( ( source, args, raw ) => {
                TriggerServerEvent( "salty::netModifyMapPos", "add", "AUTO", Convert.ToInt32(args[0]), Game.Player.Character.Position );
            } ), false );

            RegisterCommand( "addweaponpoint", new Action<int, List<object>, string>( ( source, args, raw ) => {
                TriggerServerEvent( "salty::netModifyWeaponPos", "add", "AUTO", args[0], Game.Player.Character.Position );
            } ), false );

            RegisterCommand( "kill", new Action<int, List<object>, string>( ( source, args, raw ) => {
                Game.PlayerPed.Kill();
            } ), false );

            RegisterCommand( "createmap", new Action<int, List<object>, string>( ( source, args, raw ) => {
                if( args[ 2 ] == null )
                    return;
                TriggerServerEvent( "salty::netModifyMap", "add", args[0], 0, Game.Player.Character.Position, new Vector3( float.Parse( args[1].ToString() ), float.Parse( args[2].ToString() ), 0 ) );
            } ), false );

            RegisterCommand( "street", new Action<int, List<object>, string>( ( source, args, raw ) => {
                uint streetName = 0;
                uint crossingRoad = 0;
                GetStreetNameAtCoord( Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, ref streetName, ref crossingRoad );
                Debug.WriteLine( streetName + " : " + GetStreetNameFromHashKey( streetName ) );
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
                vehicle.CanBeVisiblyDamaged = false;
                vehicle.CanEngineDegrade = false;
                vehicle.CanTiresBurst = false;
                vehicle.CanWheelsBreak = false;
                vehicle.EngineHealth = 999999;
                vehicle.MaxHealth = 999999;
                vehicle.Health = 999999;
                vehicle.EnginePowerMultiplier = 100;
                vehicle.Gravity = 50;
                

                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fCamberStiffnesss", 0.1f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fInitialDragCoeff ", 10f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fMass", 10000f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fInitialDriveForce", 2f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "FTRACTIONSPRINGDELTAMAX", 100f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fSteeringLock", 40f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fDownForceModifier", 100f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fDriveInertia", 1f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fDriveBiasFront", 0.5f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionCurveLateral", 25f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionCurveMax", 5f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionCurveMin", 5f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionBiasFront", 0.5f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fTractionLossMult", 0.1f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fSuspensionReboundDamp", 2f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fSuspensionCompDamp", 2f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "fSuspensionForce", 3f );
                SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId(vehicle.NetworkId), "CHandlingData", "FCOLLISIONDAMAGEMULT", 0f );
                SetVehicleHasStrongAxles( NetworkGetEntityFromNetworkId( vehicle.NetworkId ), true );
                SetVehicleHighGear( NetworkGetEntityFromNetworkId( vehicle.NetworkId ), 1 );


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
