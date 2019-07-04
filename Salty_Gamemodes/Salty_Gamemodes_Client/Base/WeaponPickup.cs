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
        public float WaitTime;
        public Vector3 Position;
        int WeaponID;
        public bool PlayerDropped = false;
        public int AmmoCount = 50;

        float pickupRange = 3;

        float pickupTime;
        bool canPickup;

        public WeaponPickup( Map map, string weaponModel, uint weaponHash, int worldModel, Vector3 position, bool playerDropped, float waitTime, int ammoCount ) {
            GameMap = map;
            WeaponModel = weaponModel;
            WeaponHash = weaponHash;
            WorldModel = worldModel;
            Position = position;
            PlayerDropped = playerDropped;
            WaitTime = waitTime;
            AmmoCount = ammoCount;
            if( playerDropped ) {
                canPickup = false;
                pickupTime = GetGameTimer() + waitTime;
            } else {
                canPickup = true;
            }
            WeaponID = CreateObject( worldModel, position.X, position.Y, position.Z, true, true, true );
            SetObjectPhysicsParams( WeaponID, 10, 10, 10, 3, 3, 10, 10, 10, 10, 10, 10 );
            PlaceObjectOnGroundProperly( WeaponID );
            SetObjectSomething( WeaponID, true );
            Throw();
            ActivatePhysics( WeaponID );
            map.SpawnedWeapons.Add( this );
        }

        public void Throw() {
            SetObjectPhysicsParams( WeaponID, 10, 10, 10, 3, 3, 10, 10, 10, 10, 10, 10 );
            PlaceObjectOnGroundProperly( WeaponID );
            ActivatePhysics( WeaponID );
        }

        public void Update() {
            int wepCount = 0;
            if( GameMap.Gamemode != null )
                wepCount = GameMap.Gamemode.PlayerWeapons.Count;

            if( Position.DistanceToSquared(Game.PlayerPed.Position) <= pickupRange && canPickup ) {
                if( GameMap.Gamemode.CanPickupWeapon( WeaponModel ) ) {
                    if( wepCount <= 1 ) {
                        GiveWeaponToPed( PlayerPedId(), WeaponHash, AmmoCount, false, true );
                        SetPedAmmo( PlayerPedId(), WeaponHash, AmmoCount );
                    } else {
                        GiveWeaponToPed( PlayerPedId(), WeaponHash, AmmoCount, false, false );
                        SetPedAmmo( PlayerPedId(), WeaponHash, AmmoCount );
                    }
                    Destroy();
                    canPickup = false;
                } else if (GameMap.Gamemode.HasWeapon( WeaponModel ) ) {
                    int ammo = GetAmmoInPedWeapon( PlayerPedId(), WeaponHash );
                    if( ammo + AmmoCount > GameMap.Gamemode.WeaponMaxAmmo[WeaponModel] ) {
                        int difference = GameMap.Gamemode.WeaponMaxAmmo[WeaponModel] - ammo;
                        AmmoCount -= difference;
                        if( AmmoCount <= 0 ) {
                            Destroy();
                            canPickup = false;
                            return;
                        }
                        SetPedAmmo( PlayerPedId(), WeaponHash, GameMap.Gamemode.WeaponMaxAmmo[WeaponModel] );
                    } else {
                        SetPedAmmo( PlayerPedId(), WeaponHash, ammo + AmmoCount );
                        Destroy();
                        canPickup = false;
                        return;
                    } 
                }
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
