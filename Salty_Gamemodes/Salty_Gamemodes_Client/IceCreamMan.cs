using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;

namespace Salty_Gamemodes_Client {
    class IceCreamMan : BaseGamemode {

        Text streetText;

        Vehicle Truck;
        Vehicle Bike;
        Random rand;

        bool canKill = false;


        List<string> Bikes = new List<string>() {
            "BMX",
            "Cruiser",
            "Fixter",
            "Scorcher",
            "TriBike"
        };

        public enum Teams {
            Spectators,
            Driver,
            Runner
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public override void Start() {
            rand = new Random( GetGameTimer() );
            Game.Player.Character.MaxHealth = 100;
            Game.Player.Character.Health = 100;
            if( Team == 1 ) {
                SpawnTruck();
            }
            if( Team == 2 ) {
                SpawnBike();
            }
            WriteChat( "Game starting" );
            base.Start();
        }

        public IceCreamMan( Map gameMap, int team ) {

            streetText = new Text( "", new System.Drawing.PointF( 0.5f, 0.5f ), 1f );

            GameWeapons = new Dictionary<string, string>() {
                { "WEAPON_UNARMED", "Fists" },
                { "WEAPON_RPG", "Bazooka" }
            };

            WeaponSlots = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 0 },
                { "WEAPON_RPG", 1  },
            };

            WeaponMaxAmmo = new Dictionary<string, int>() {
                { "WEAPON_UNARMED", 1  },
                { "WEAPON_RPG", 100  },
            };


            GameMap = gameMap;
            GameMap.Gamemode = this;

            SetTeam( team );
        }

        public override void PlayerPickedUpWeapon( string wepName, int count ) {
            WriteChat( "Picked up " + wepName );
            base.PlayerPickedUpWeapon( wepName, count );
        }


        public override void Update() {
            if( Team == 1 ) {
                if( !Game.PlayerPed.IsInVehicle() ) {
                    //Game.PlayerPed.SetIntoVehicle( Truck, VehicleSeat.Driver );
                }
                uint streetName = 0;
                uint crossingName = 0;
                GetStreetNameAtCoord( Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, ref streetName, ref crossingName );
                if( streetName == 629262578 || crossingName == 629262578 ) {
                    var velocity = Truck.Velocity;
                    var speed = Truck.Speed;
                    Truck.Position = PlayerSpawn;
                    Truck.Heading = 67.7f;
                    Truck.Velocity = velocity * 2;
                    Truck.Speed = speed * 1.5f;
                    SetGameplayCamRelativeHeading( 0 );
                    Score++;
                    TriggerServerEvent( "salty::netAddScore", 1 );
                }
            }
            if( Team == 2 && !canKill) {
                if( !Game.PlayerPed.IsInVehicle()  ) {
                    //Game.PlayerPed.SetIntoVehicle( Bike, VehicleSeat.Driver );
                }
                uint streetName = 0;
                uint crossingName = 0;
                GetStreetNameAtCoord( Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, ref streetName, ref crossingName );
                if( streetName == 3436239235 || crossingName == 3436239235 ) {
                    canKill = true;
                    GiveWeaponToPed( PlayerPedId(), (uint)GetHashKey( "weapon_rpg" ), 100, false, true );
                    
                }
            }

            if( canKill ) {
                //Game.Player.SetRunSpeedMultThisFrame( 20 );
            }

            base.Update();

        }

        public override void End() {
            if( Truck != null )
                Truck.Delete();
            if( Bike != null )
                Bike.Delete();


            base.End();
        }

        public void Respawn() {
            if( Team == 1 ) {
                SpawnTruck();
            }
            if( Team == 2 ) {
                SpawnBike();
            }
        }
        

        public override void PlayerSpawned( ExpandoObject spawnInfo ) {
            Respawn();
        }

        public async Task SpawnBike() {
            if( Bike != null )
                Bike.Delete();
            Game.PlayerPed.Position = PlayerSpawn;
            Bike = await World.CreateVehicle( Bikes[rand.Next(0, Bikes.Count)], Game.PlayerPed.Position, 266.6f );
            Bike.MaxSpeed = 15;
            Game.PlayerPed.SetIntoVehicle( Bike, VehicleSeat.Driver );
        }

        public async Task SpawnTruck() {
            if( Truck != null )
                Truck.Delete();
            Game.PlayerPed.Position = PlayerSpawn;
            Truck = await World.CreateVehicle( "cutter", Game.PlayerPed.Position, 67.7f );
            Truck.CanBeVisiblyDamaged = false;
            Truck.CanEngineDegrade = false;
            Truck.CanTiresBurst = false;
            Truck.CanWheelsBreak = false;
            Truck.EngineHealth = 999999;
            Truck.MaxHealth = 999999;
            Truck.Health = 999999;
            Truck.EnginePowerMultiplier = 100;
            Truck.Gravity = 50;
            Truck.IsInvincible = true;
            Truck.IsFireProof = true;

            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fCamberStiffnesss", 0.1f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fInitialDragCoeff ", 10f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fMass", 10000f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fInitialDriveForce", 2f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "FTRACTIONSPRINGDELTAMAX", 100f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fSteeringLock", 40f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fDownForceModifier", 100f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fDriveInertia", 1f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fDriveBiasFront", 0.5f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fTractionCurveLateral", 25f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fTractionCurveMax", 5f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fTractionCurveMin", 5f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fTractionBiasFront", 0.5f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fTractionLossMult", 0.1f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fSuspensionReboundDamp", 2f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fSuspensionCompDamp", 2f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fSuspensionForce", 3f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "FCOLLISIONDAMAGEMULT", 0f );
            SetVehicleHasStrongAxles( NetworkGetEntityFromNetworkId( Truck.NetworkId ), true );
            SetVehicleHighGear( NetworkGetEntityFromNetworkId( Truck.NetworkId ), 1 );

            Game.PlayerPed.SetIntoVehicle( Truck, VehicleSeat.Driver );

            SetGameplayCamRelativeHeading( 0 );

        }

        public override void HUD() {
            uint streetName = 0;
            uint crossingRoad = 0;
            GetStreetNameAtCoord( Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, ref streetName, ref crossingRoad );
            streetText.Caption = GetStreetNameFromHashKey( streetName );
            streetText.Draw();

            HideHudAndRadarThisFrame();
            DrawBaseWeaponHUD();

            base.HUD();
        }
    }
}
