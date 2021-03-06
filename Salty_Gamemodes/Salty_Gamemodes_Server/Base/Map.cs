﻿using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    public class Map : BaseScript {

        public string Name = "None";
        public bool isActive = false;
        public Vector3 Position;
        public Vector3 Size;

        public Dictionary<int, List<Vector3>> SpawnPoints = new Dictionary<int, List<Vector3>>();
        public Dictionary<string, List<Vector3>> GunSpawns = new Dictionary<string, List<Vector3>>();

        public Dictionary<string, string> GameWeapons = new Dictionary<string, string>();

        public Dictionary<string, int> WeaponSpawnAmmo = new Dictionary<string, int>() {
            { "WEAPON_UNARMED", 0 },
        };

        private Dictionary<int, List<Vector3>> usedSpawns = new Dictionary<int, List<Vector3>>();
        private Random rand;


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

        Dictionary<int, string> weaponIntervals = new Dictionary<int, string>();


        public Map( Vector3 position, Vector3 size, string name) {
            rand = new Random(DateTime.Now.Millisecond);
            usedSpawns = new Dictionary<int, List<Vector3>>();
            Position = position;
            Size = size;
            Name = name;
        }

        public bool IsInZone( Vector3 pos ) {
            return ( pos.X > Position.X - ( Size.X / 2 ) && pos.X < Position.X + ( Size.X / 2 ) && pos.Y > Position.Y - ( Size.Y / 2 ) && pos.Y < Position.Y + ( Size.Y / 2 ) );
        }

        public void ResetSpawns() {
            usedSpawns = new Dictionary<int, List<Vector3>>();
        }

        public Vector3 GetNextSpawn( int team ) {
            if( !usedSpawns.ContainsKey( team ) || usedSpawns[team].Count == 0 ) {
                int index = rand.Next( 0, SpawnPoints[team].Count );
                return SpawnPoints[team][index]; 
            } else {
                int index = rand.Next(0, usedSpawns[team].Count);
                Vector3 spawn = usedSpawns[team][index];
                usedSpawns.Remove(index);
                return spawn;
            }
        }

        public void AddSpawnPoint( int team, Vector3 spawnPoint ) {
            if( SpawnPoints.ContainsKey( team ) ) {
                SpawnPoints[team].Add( spawnPoint );
            }
            else {
                SpawnPoints.Add( team, new List<Vector3> { spawnPoint } );
            }
            if (!usedSpawns.ContainsKey(team))
                usedSpawns.Add(team, new List<Vector3>());
            usedSpawns[team].Add(spawnPoint);
        }

        public void DeleteSpawnPoint( int team, Vector3 spawnPoint ) {
            SpawnPoints[team].Remove( spawnPoint );
        }


        public void AddWeaponSpawn( string weaponType, Vector3 spawnPoint ) {
            if( GunSpawns.ContainsKey(weaponType) ) {
                GunSpawns[weaponType].Add( spawnPoint );
            } else {
                GunSpawns.Add( weaponType, new List<Vector3> { spawnPoint } );
            }
        }

        public void DeleteWeaponSpawn( string weaponType, Vector3 pos ) {
            GunSpawns[weaponType].Remove( pos );
        }

        public string SpawnPointsAsString( ) {
            string spawnPoints = "";

            foreach( var vector in SpawnPoints ) {
                foreach( var spawn in vector.Value ) {
                    spawnPoints += string.Format( "{0},{1},{2},{3}:", vector.Key, spawn.X, spawn.Y, spawn.Z );
                }
            }

            if( spawnPoints == "" ) {
                spawnPoints = string.Format("0,{0},{1},{2}:", Position.X, Position.Y, Position.Z);
            }
            return spawnPoints.Substring( 0, spawnPoints.Length - 1 );
        }


        public void SpawnWeapons() {
            if( GameWeapons == null )
                return;

            Random rand = new Random( DateTime.Now.Millisecond );


            weaponIntervals = new Dictionary<int, string>();
            int i = 0;
            foreach( var wep in GameWeapons ) {
                if( wep.Key == "WEAPON_UNARMED" || !WeaponWeights.ContainsKey( wep.Key ) )
                    continue;
                i += WeaponWeights[wep.Key];
                weaponIntervals.Add( i, wep.Key );
            }

            foreach( var gunTypes in GunSpawns ) {
                foreach( var gunPos in gunTypes.Value ) {
                    if( gunTypes.Key == "random" || !Weapons.ContainsKey( gunTypes.Key ) ) {

                        int index = rand.Next( 0, i );
                        string prevItem = Weapons.ElementAt( 0 ).Key;
                        string wepModel = Weapons.ElementAt( Weapons.Count - 1 ).Key, worldModel = Weapons.ElementAt( Weapons.Count - 1 ).Value;
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
                        TriggerClientEvent( "salty::SpawnWeapon", wepModel, pickupHash, worldHash, gunPos, false, 0, WeaponSpawnAmmo[wepModel], -1 );
                    }
                    else
                        TriggerClientEvent( "salty::SpawnWeapon", gunTypes.Key, (uint)GetHashKey( gunTypes.Key ), GetHashKey( Weapons[gunTypes.Key] ), gunPos, false, 0, WeaponSpawnAmmo[gunTypes.Key], -1 );
                }
            }
        }

        public string GunSpawnsAsString( ) {
            string gunSpawns = "";
            foreach( var vector in GunSpawns ) {
                foreach( var spawn in vector.Value ) {
                    gunSpawns += string.Format( "{0},{1},{2},{3}:", vector.Key, spawn.X, spawn.Y, spawn.Z );
                }
            }
            if( gunSpawns == "" ) {
                gunSpawns = "random,0,0,0:";
            }
            return gunSpawns.Substring( 0, gunSpawns.Length - 1 );
        }

        public void SendToClients( List<Player> players ) {
            foreach( var ply in players ) {
                ply.TriggerEvent( "salty::CreateMap", Name, Position, Size );
            }
        }

        public static Map None() {
            return new Map( new Vector3( 0, 0, 0 ), new Vector3( 0, 0, 0 ), "____" );
        }

    }
}
