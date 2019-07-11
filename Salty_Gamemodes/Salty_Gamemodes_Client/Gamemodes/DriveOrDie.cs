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
    class DriveOrDie : BaseGamemode {

        float justLeftMap = 0;
        Vehicle Truck;
        Vehicle Bike;

        

        List<string> Bikes = new List<string>() {
            "Akuma",
            "Avarus",
            "Bagger",
            "Bati",
            "BF400",
            "Blazer4",
            "Chimera",
            "Thrust",
            "Sanchez2",
            "Fcr"
        };

        public enum Teams {
            Spectators,
            Trucker,
            Bikie
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        Player targetPlayer;
        List<Player> playerList = new PlayerList().ToList();


        public DriveOrDie( Map gameMap, int team ) {

            GameWeapons = new Dictionary<string, string>() {
                { "WEAPON_UNARMED", "Fists" }
            };

            GameMap = gameMap;
            GameMap.Gamemode = this;
            GameMap.CreateBlip();

            SetTeam( team );
        }


        public override void HUD() {
            HideHudAndRadarThisFrame();
            base.HUD();
        }

        public override void Start() {

            Game.PlayerPed.IsInvincible = true;
            playerList = new PlayerList().ToList();
            playerList.Remove( Game.Player );
            ActiveHUD.SetGameTimePosition(0, 0, false);
            if ( Team == (int)Teams.Trucker ) {
                ActiveHUD.SetGoal( "Destroy all the bikes", 230, 0, 0, 255, 5 );
                SpawnTruck();
            } else {
                ActiveHUD.SetGoal( "Dodge the Monster Trucks\nAvoid getting stuck in water and other vehicles or KABOOM!", 0, 230, 0, 255, 5 );
                SpawnBike();
            }

            base.Start();
        }

        Random rand = new Random();
        public override void Update() {
            CantExitVehichles();
            if( Bike != null ) {
                if( Bike.IsInWater && !Bike.IsEngineRunning ) {
                    ExplodeVehicle( Bike );
                    Bike = null;
                }
                if( Bike.HasCollided ) {
                    if( Bike.MaterialCollidingWith.ToString() == "CarMetal" ) {
                        ExplodeVehicle( Bike );
                        Bike = null;
                    }
                }
            }
            if( Truck != null ) {
                if( Truck.IsInWater && !Truck.IsEngineRunning ) {
                    ExplodeVehicle( Truck );
                    Truck = null;
                }
            }

            if( !Game.PlayerPed.IsInVehicle() ) {
                if( Team == (int)Teams.Bikie && Bike != null ) 
                    Game.PlayerPed.SetIntoVehicle( Bike, VehicleSeat.Driver );
                if( Team == (int)Teams.Trucker && Truck != null )
                    Game.PlayerPed.SetIntoVehicle( Truck, VehicleSeat.Driver );

            }

            if( !GameMap.IsInZone( Game.PlayerPed.Position ) ) {
                justLeftMap = GetGameTimer();
            }

            if( justLeftMap + 1000 > GetGameTimer() ) {
                if( Game.PlayerPed.IsInVehicle() ) {

                    Vector3 direction;
                    if( playerList.Count == 0 ) {
                        direction = Game.PlayerPed.CurrentVehicle.Position - GameMap.Position;
                    }
                    else {
                        targetPlayer = targetPlayer == null ? playerList[rand.Next( playerList.Count )] : targetPlayer;
                        direction = Game.PlayerPed.CurrentVehicle.Position - targetPlayer.Character.Position;
                    }

                    direction.Z = rand.Next( 300, 500 );
                    direction.Y = -direction.Y;
                    direction.X = -direction.X;
                    Game.PlayerPed.CurrentVehicle.ApplyForce( direction, default, ForceType.MaxForceRot );

                }
            }
            if( justLeftMap + 1100 < GetGameTimer() && targetPlayer != null ) {
                targetPlayer = null;
            }

            base.Update();
        }

        public void ExplodeVehicle( Vehicle veh ) {
            Game.PlayerPed.IsInvincible = false;
            Game.Player.IsInvincible = false;
            Game.Player.Character.Kill();
            veh.IsInvincible = false;
            veh.IsExplosionProof = false;
            veh.ExplodeNetworked();
        }

        public override void PlayerSpawned( ExpandoObject spawnInfo ) {
            if( Team == (int)Teams.Bikie ) {
                ActiveHUD.SetGoal("Destroy all the bikes", 230, 0, 0, 255, 5);
                Bike.Delete();
                SetTeam( (int)Teams.Trucker );
            }
            Game.PlayerPed.IsInvincible = true;
            SpawnTruck();
            base.PlayerSpawned( spawnInfo );
        }

        public async Task SpawnBike() {
            if( Bike != null )
                Bike.Delete();
            Game.PlayerPed.Position = PlayerSpawn;
            Bike = await World.CreateVehicle( Bikes[rand.Next( 0, Bikes.Count )], Game.PlayerPed.Position, 266.6f );
            if( Bike != null )
                Game.PlayerPed.SetIntoVehicle( Bike, VehicleSeat.Driver );
            SetGameplayCamRelativeHeading( 0 );
            Game.PlayerPed.CanBeKnockedOffBike = false;
        }

        public async Task SpawnTruck() {
            if( Truck != null )
                Truck.Delete();
            Game.PlayerPed.Position = PlayerSpawn;
            Truck = await World.CreateVehicle( "monster", Game.PlayerPed.Position, 67.7f );
            Truck.CanBeVisiblyDamaged = false;
            Truck.CanEngineDegrade = false;
            Truck.CanTiresBurst = false;
            Truck.CanWheelsBreak = false;
            Truck.EngineHealth = 999999;
            Truck.MaxHealth = 999999;
            Truck.Health = 999999;
            Truck.EnginePowerMultiplier = 100;
            Truck.Gravity = 35;
            Truck.IsInvincible = true;
            Truck.IsFireProof = true;

            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fCamberStiffnesss", 0.1f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fInitialDragCoeff ", 10f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fMass", 10000f );
            SetVehicleHandlingFloat( NetworkGetEntityFromNetworkId( Truck.NetworkId ), "CHandlingData", "fSteeringLock", 40f );
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
            SetVehicleHasStrongAxles( NetworkGetEntityFromNetworkId( Truck.NetworkId ), true );
            SetVehicleHighGear( NetworkGetEntityFromNetworkId( Truck.NetworkId ), 1 );

            Game.PlayerPed.SetIntoVehicle( Truck, VehicleSeat.Driver );

            SetGameplayCamRelativeHeading( 0 );

        }

        public override void End() {
            if( Truck != null )
                Truck.Delete();
            if( Bike != null )
                Bike.Delete();
            base.End();
        }
    }
}
