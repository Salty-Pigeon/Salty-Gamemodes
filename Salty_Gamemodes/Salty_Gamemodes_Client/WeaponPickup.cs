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

        public string WeaponModel;
        public uint WeaponHash;
        public int WorldModel;
        public Vector3 Position;
        int WeaponID;
        public bool PlayerDropped = false;

        float pickupRange = 3;

        float pickupTime;
        bool canPickup;

        public WeaponPickup( Map map, string weaponModel, uint weaponHash, int worldModel, Vector3 position, bool playerDropped ) {
            GameMap = map;
            WeaponModel = weaponModel;
            WeaponHash = weaponHash;
            WorldModel = worldModel;
            Position = position;
            PlayerDropped = playerDropped;
            if( playerDropped ) {
                canPickup = false;
                pickupTime = GetGameTimer() + (2 * 1000);
            } else {
                canPickup = true;
            }
            WeaponID = CreateObject( worldModel, position.X, position.Y, position.Z, true, true, true );
            SetObjectPhysicsParams( WeaponID, 20, 20, 20, 1, 1, 20, 20, 20, 20, 20, 20 );
            PlaceObjectOnGroundProperly( WeaponID );
            SetObjectSomething( WeaponID, true );
            Throw();
        }

        public void Throw() {
            SetObjectPhysicsParams( WeaponID, 20, 20, 20, 3, 3, 20, 20, 20, 20, 20, 20 );
            PlaceObjectOnGroundProperly( WeaponID );
        }

        public void Update() {
            int wepCount = 0;
            if( GameMap.Gamemode != null )
                wepCount = GameMap.Gamemode.PlayerWeapons.Count;
            bool canPickupEvent = true;
            if( GameMap.Gamemode != null )
                canPickupEvent = GameMap.Gamemode.CanPickupWeapon( WeaponModel );

            if( Position.DistanceToSquared(Game.PlayerPed.Position) <= pickupRange && wepCount < 2 && canPickup && canPickupEvent ) {
                GiveWeaponToPed( PlayerPedId(), WeaponHash, 50, false, true );
                Destroy();
            }

            if( GetGameTimer() - pickupTime > 0 ) {
                canPickup = true;
            }
        }

        public void Destroy() {
            DeleteObject( ref WeaponID );
            GameMap.RemoveWeapon( this );
        }
    }
}
