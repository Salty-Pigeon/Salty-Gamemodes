using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    public class WeaponPickup : BaseScript {

        Map GameMap;

        public uint WeaponHash;
        public int WorldModel;
        public Vector3 Position;
        int WeaponID;

        float pickupRange = 3;

        float pickupTime;
        bool canPickup;

        public WeaponPickup( Map map, uint weaponHash, int worldModel, Vector3 position, bool playerDropped ) {
            GameMap = map;
            WeaponHash = weaponHash;
            WorldModel = worldModel;
            Position = position;
            if( playerDropped ) {
                canPickup = false;
                pickupTime = GetGameTimer() + (3 * 1000);
            } else {
                canPickup = true;
            }
            WeaponID = CreateObject( worldModel, position.X, position.Y, position.Z, true, true, true );
            SetObjectPhysicsParams( WeaponID, 20, 20, 20, 1, 1, 20, 20, 20, 20, 20, 20 );
            PlaceObjectOnGroundProperly( WeaponID );
            SlideObject( WeaponID, position.X, position.Y, position.Z - 1, 1, 1, 1, true );
            SetObjectSomething( WeaponID, true );
            SetForceObjectThisFrame( WeaponID, 1, 1, 1 );
        }

        public void Throw() {
            SlideObject( WeaponID, Position.X, Position.Y + 1, Position.Z + 0.5f, 1, 1, 1, true );
        }
        public void Update() {
            if( Position.DistanceToSquared(Game.PlayerPed.Position) <= pickupRange && BaseGamemode.WeaponCount < 2 && canPickup  ) {
                GiveWeaponToPed( PlayerPedId(), WeaponHash, 50, false, true );
                Destroy();
            }

            if( GetGameTimer() - pickupTime > 1 * 1000 ) {
                canPickup = true;
            }

        }

        public void Destroy() {
            DeleteObject( ref WeaponID );
            GameMap.RemoveWeapon( this );
        }
    }
}
