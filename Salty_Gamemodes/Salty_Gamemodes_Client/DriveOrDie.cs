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

        public override void Start() {
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

            if( justLeftMap + 500 > GetGameTimer() ) {
                if( Game.PlayerPed.IsInVehicle() ) {
                    Vector3 direction = Game.PlayerPed.CurrentVehicle.Position - GameMap.Position;
                    direction.Z = rand.Next( 80, 120 );
                    direction.Y = -direction.Y;
                    direction.X = -direction.X;
                    Game.PlayerPed.CurrentVehicle.ApplyForce( direction, default, ForceType.MaxForceRot );

                }
                //ApplyForceToEntityCenterOfMass( PlayerPedId(), (int)ForceType.MaxForceRot, GameMap.Position.X, -GameMap.Position.Y, GameMap.Position.Z + 300, false, true, true, true );
                //ApplyForceToEntity( PlayerPedId(), (int)ForceType.MaxForceRot, Game.PlayerPed.Position.X, -Game.PlayerPed.Position.Y, 200, -GameMap.Position.X, -GameMap.Position.Y, 200, (int)Bone.SKEL_ROOT, false, true, true, true, true );

            }

            base.Update();
        }

        public override void End() {
            base.End();
        }
    }
}
