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

        public Dictionary<string, int> WeaponWeights = new Dictionary<string, int>();

        public Dictionary<string, WeaponPickup> CreatedWeapons = new Dictionary<string, WeaponPickup>();

        Dictionary<int, string> weaponIntervals = new Dictionary<int, string>();

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

        public void SpawnWeapons() {

            Random rand = new Random( DateTime.Now.Millisecond );


            weaponIntervals = new Dictionary<int, string>();
            int i = 0;
            foreach( var wep in Gamemode.GameWeapons ) {
                if( wep.Key == "WEAPON_UNARMED" || !WeaponWeights.ContainsKey( wep.Key ) )
                    continue;
                i += WeaponWeights[wep.Key];
                weaponIntervals.Add( i, wep.Key );
            }

            foreach( var gunTypes in GunSpawns ) {
                foreach( var gunPos in gunTypes.Value ) {
                    if( gunTypes.Key == "random" || !Weapons.ContainsKey(gunTypes.Key) ) {

                        int index = rand.Next( 0, i );
                        string prevItem = Weapons.ElementAt( 0 ).Key;
                        string wepModel = Weapons.ElementAt( Weapons.Count-1 ).Key, worldModel = Weapons.ElementAt( Weapons.Count-1 ).Value ;
                        int prevKey = 0;
                        foreach( var x in weaponIntervals ) {      
                            
                            if( index > prevKey && index <= x.Key ) {
                                wepModel = x.Value;
                                worldModel = Weapons[x.Value];
                                break;
                            }

                            prevItem = x.Value;
                            prevKey = x.Key;
                        }
                        uint pickupHash = (uint)GetHashKey( wepModel );
                        int worldHash = GetHashKey( worldModel );
                        WeaponPickup item = new WeaponPickup( this, wepModel, pickupHash, worldHash, gunPos, false, 0, Gamemode.WeaponMaxAmmo[wepModel]/3, -1 );
                        if( !CreatedWeapons.ContainsKey( wepModel ) )
                            CreatedWeapons.Add( wepModel, item );
                    }
                    else {
                        WeaponPickup item = new WeaponPickup( this, gunTypes.Key, ( uint)GetHashKey( gunTypes.Key ), GetHashKey( Weapons[gunTypes.Key] ), gunPos, false, 0, Gamemode.WeaponMaxAmmo[gunTypes.Key] / 3, -1 );
                        if( !CreatedWeapons.ContainsKey( gunTypes.Key ) )
                            CreatedWeapons.Add( gunTypes.Key, item );
                    }

                }
            }
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
        }

        public bool IsInZone( Vector3 pos ) {
            return ( pos.X > Position.X - ( Size.X / 2 ) && pos.X < Position.X + ( Size.X / 2 ) && pos.Y > Position.Y - ( Size.Y / 2 ) && pos.Y < Position.Y + ( Size.Y / 2 ) );
        }
    }
}
