using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    public class Map : BaseScript {


        List<string> spawnedWeps = new List<string>();

        Dictionary<string, string> weapons = new Dictionary<string, string>(){
            { "W_PI_PISTOL", "PICKUP_WEAPON_PISTOL" },
            { "W_PI_COMBATPISTOL", "PICKUP_WEAPON_COMBATPISTOL"  },
            { "W_SB_SMG", "PICKUP_WEAPON_SMG"  },
            { "W_AR_CARBINERIFLE", "PICKUP_WEAPON_CARBINERIFLE"  },
            { "W_AR_ASSAULTRIFLE", "PICKUP_WEAPON_ASSAULTRIFLE"  },
            { "W_SR_SNIPERRIFLE", "PICKUP_WEAPON_SNIPERRIFLE" },
            { "W_SG_PUMPSHOTGUN", "PICKUP_WEAPON_PUMPSHOTGUN"  },
            { "W_SB_MICROSMG", "PICKUP_WEAPON_MICROSMG" },
            { "W_SG_SAWNOFF", "PICKUP_WEAPON_SAWNOFFSHOTGUN" }
        };

        List<int> weaponIDs = new List<int>();

        public int Blip = -1;

        public Vector3 Position;
        public Vector3 Size;
        public string Name;

        public List<Vector3> SpawnPoints = new List<Vector3>();
        public Dictionary<string, List<Vector3>> GunSpawns = new Dictionary<string, List<Vector3>>();


        public bool isActive = false;

        public bool isVisible = false;

        public Map( Vector3 position, Vector3 size, string name ) {
            Position = position;
            Size = size;
            Name = name;

            foreach( var wep in weapons ){
                spawnedWeps.Add( wep.Key );
            }

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
        }

        public void SpawnWeapons() {
            foreach( var gunTypes in GunSpawns ) {
                foreach( var gunPos in gunTypes.Value ) {
                    if( gunTypes.Key == "random" || !weapons.ContainsKey(gunTypes.Key) ) {
                        Random rand = new Random();
                        int index;
                        string worldModel, pickup;
                        if( spawnedWeps.Count == 0 ) {
                            index = rand.Next( 0, weapons.Count );
                            worldModel = weapons.ElementAt( index ).Key;
                            pickup = weapons.ElementAt( index ).Value;

                        }
                        else {
                            index = rand.Next( 0, spawnedWeps.Count );
                            worldModel = spawnedWeps[index];
                            pickup = weapons[worldModel];

                            spawnedWeps.Remove( worldModel );
                        }
                        Debug.WriteLine( string.Format( "pickup {0} spawning with model {1}", pickup, worldModel ));
                        int item = CreatePickup( (uint)GetHashKey( pickup ), gunPos.X, gunPos.Y, gunPos.Z, 10, 10, true, (uint)GetHashKey( worldModel ) );
                        weaponIDs.Add( item );

                    }
                    else {
                        int item = CreatePickup( (uint)GetHashKey( weapons[gunTypes.Key] ), gunPos.X, gunPos.Y, gunPos.Z, 10, 10, true, (uint)GetHashKey( gunTypes.Key ) );
                        SetObjectPhysicsParams( item, 1, 1, 1, 1, 1, 5, 1, 1, 1, 1, 1 );
                        weaponIDs.Add( item );
                    }

                }
            }
        }

        public void ClearWeapons() {
            foreach( int obj in weaponIDs ) {
                RemovePickup( obj );
            }
        }

        public void DrawSpawnPoints() {
            foreach(Vector3 spawnCoords in SpawnPoints ) {
                DrawMarker( 2, spawnCoords.X, spawnCoords.Y, spawnCoords.Z, 0.0f, 0.0f, 0.0f, 0.0f, 180.0f, 0.0f, 2.0f, 2.0f, 2.0f, 255, 128, 0, 200, false, true, 2, false, null, null, false );
            }
        }

        public void ClearBlip() {
            RemoveBlip( ref Blip );
            isVisible = false;
        }

        public bool IsInZone( Vector3 pos ) {
            return ( pos.X > Position.X - ( Size.X / 2 ) && pos.X < Position.X + ( Size.X / 2 ) && pos.Y > Position.Y - ( Size.Y / 2 ) && pos.Y < Position.Y + ( Size.Y / 2 ) );
        }
    }
}
