﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    public class Map : BaseScript {

        public BaseGamemode Gamemode;

        public Dictionary<string, string> Weapons = new Dictionary<string, string>(){
            { "WEAPON_PISTOL", "W_PI_PISTOL"  },
            { "WEAPON_COMBATPISTOL", "W_PI_COMBATPISTOL" },
            { "WEAPON_SMG", "W_SB_SMG" },
            { "WEAPON_CARBINERIFLE", "W_AR_CARBINERIFLE"  },
            { "WEAPON_ASSAULTRIFLE", "W_AR_ASSAULTRIFLE"  },
            { "WEAPON_SNIPERRIFLE", "W_SR_SNIPERRIFLE" },
            { "WEAPON_PUMPSHOTGUN", "W_SG_PUMPSHOTGUN"  },
            { "WEAPON_MICROSMG", "W_SB_MICROSMG" },
            { "WEAPON_COMBATMG", "W_MG_COMBATMG" }
        };

        public Dictionary<string, WeaponPickup> CreatedWeapons = new Dictionary<string, WeaponPickup>();

        public List<WeaponPickup> SpawnedWeapons = new List<WeaponPickup>();


        public int Blip = -1;

        public Vector3 Position;
        public Vector3 Size;
        public string Name;

        public Dictionary<int, List<Vector3>> SpawnPoints = new Dictionary<int, List<Vector3>>();
        public Dictionary<string, List<Vector3>> GunSpawns = new Dictionary<string, List<Vector3>>();


        public bool isActive = false;

        public bool isVisible = false;

        public Map( Vector3 position, Vector3 size, string name ) {
            Position = position;
            Size = size;
            Name = name;       
   
        }

        public void CreateBlip() {
            Blip = AddBlipForArea( Position.X, Position.Y, Position.Z, Size.X, Size.Y );
            SetBlipAsShortRange( Blip, true );
            SetBlipColour( Blip, 2 );
            SetBlipSprite( Blip, 398 );
            SetBlipRotation( Blip, 0 );
            BeginTextCommandSetBlipName( "STRING" );
            AddTextComponentString( "Map bounds" );
            EndTextCommandSetBlipName( Blip );
            isVisible = true;
            Debug.WriteLine( "Blip created" );
        }

        public void Update() {
            foreach( var wep in SpawnedWeapons.ToList() ) {
                wep.Update();
            }
        }
       

        public void DrawBoundarys() {

            // Top box
            DrawBox( Position.X - (Size.X / 2), Position.Y - (Size.Y / 2), 0, Position.X + (Size.X / 2), Position.Y - (Size.Y / 2) - 0.1f, 1000, 255, 255, 255, 50 ); 

            // Left box
            DrawBox( Position.X - (Size.X / 2), Position.Y - (Size.Y / 2), 0, Position.X - (Size.X / 2) - 0.1f, Position.Y + (Size.Y / 2), 1000, 255, 255, 255, 50 );

            // Right box
            DrawBox( Position.X + (Size.X / 2), Position.Y + (Size.Y / 2), 0, Position.X + (Size.X / 2) + 0.1f, Position.Y - (Size.Y / 2), 1000, 255, 255, 255, 50 );

            // Bottom box
            DrawBox( Position.X - (Size.X / 2), Position.Y + (Size.Y / 2), 0, Position.X + (Size.X / 2), Position.Y + (Size.Y / 2) + 0.1f, 1000, 255, 255, 255, 50 );

            // Roof
            DrawBox( Position.X - (Size.X / 2), Position.Y - (Size.Y / 2), 1000, Position.X + (Size.X / 2), Position.Y + (Size.Y / 2), 1000.1f, 255, 255, 255, 50 );
        }

        public void RemoveWeapon(WeaponPickup item) {
            SpawnedWeapons.Remove( item );
        }

        public void SpawnWeapon( string wepModel, uint pickupHash, int worldHash, Vector3 gunPos, bool playerDropped, float waitTime, int ammoCount, int ammoInClip ) {
            WeaponPickup item = new WeaponPickup( this, wepModel, pickupHash, worldHash, gunPos, playerDropped, waitTime, ammoCount, ammoInClip );
            if( !CreatedWeapons.ContainsKey( wepModel ) )
                CreatedWeapons.Add( wepModel, item );
        }

        public void ClearWeapons() {

            foreach( var wep in SpawnedWeapons.ToList() ) {
                try {
                    wep.Destroy();
                } catch {
                    Debug.WriteLine( string.Format("{0} failed to delete, player dropped? {1}", wep.WorldModel, wep.PlayerDropped ));
                }
            }

            
        }

        public void ClearObjects() {
            ClearAreaOfObjects( Position.X, Position.Y, Position.Z, Size.X + Size.Y, 0 );
            ClearAreaOfProjectiles( Position.X, Position.Y, Position.Z, Size.X + Size.Y, true );
        }


        public void DrawSpawnPoints() {
            int i = 0;
            foreach(var spawns in SpawnPoints ) {             
                foreach( var vector in spawns.Value ) {
                    DrawMarker( 2, vector.X, vector.Y, vector.Z, 0.0f, 0.0f, 0.0f, 0.0f, 180.0f, 0.0f, 2.0f, 2.0f, 2.0f, i / 10 * 6, i, i / 3, 200, false, true, 2, false, null, null, false ); 
                }
                i += 50;
            }
        }

        public void ClearBlip() {
            if( isVisible ) {
                RemoveBlip( ref Blip );
                isVisible = false;
            }
            ClearObjects();
        }

        public bool IsInZone( Vector3 pos ) {
            return ( pos.X > Position.X - ( Size.X / 2 ) && pos.X < Position.X + ( Size.X / 2 ) && pos.Y > Position.Y - ( Size.Y / 2 ) && pos.Y < Position.Y + ( Size.Y / 2 ) );
        }
    }
}
