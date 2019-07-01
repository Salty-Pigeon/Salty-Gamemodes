using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class DriveOrDie : BaseGamemode {

        float justLeftMap = 0;
        Vehicle Truck;

        public enum Teams {
            Spectators,
            Driver,
            Runnder
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

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

            base.HUD();
        }

        public override void Start() {
            SpawnTruck();
            Game.PlayerPed.IsInvincible = true;
            base.Start();
        }

        Random rand = new Random();
        public override void Update() {

            if( Game.PlayerPed.IsInVehicle() ) {
                //Game.PlayerPed.CurrentVehicle.EngineTorqueMultiplier = 200;
            }

            if( !GameMap.IsInZone( Game.PlayerPed.Position ) ) {
                justLeftMap = GetGameTimer();
            }

            if( justLeftMap + 1000 > GetGameTimer() ) {
                if( Game.PlayerPed.IsInVehicle() ) {
                    // Do random person here
                    Vector3 direction = Game.PlayerPed.CurrentVehicle.Position - GameMap.Position;
                    direction.Z = rand.Next( 300, 500 );
                    direction.Y = -direction.Y;
                    direction.X = -direction.X;
                    Game.PlayerPed.CurrentVehicle.ApplyForce( direction, default, ForceType.MaxForceRot );

                }
                //ApplyForceToEntityCenterOfMass( PlayerPedId(), (int)ForceType.MaxForceRot, GameMap.Position.X, -GameMap.Position.Y, GameMap.Position.Z + 300, false, true, true, true );
                //ApplyForceToEntity( PlayerPedId(), (int)ForceType.MaxForceRot, Game.PlayerPed.Position.X, -Game.PlayerPed.Position.Y, 200, -GameMap.Position.X, -GameMap.Position.Y, 200, (int)Bone.SKEL_ROOT, false, true, true, true, true );

            }

            base.Update();
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

        public override void End() {
            if( Truck != null )
                Truck.Delete();
            base.End();
        }
    }
}
