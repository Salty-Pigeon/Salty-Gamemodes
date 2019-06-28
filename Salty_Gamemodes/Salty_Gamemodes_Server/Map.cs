using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
    class Map {

        public string Name = "None";
        public bool isActive = false;
        public Vector3 Position;
        public Vector3 Size;

        public Dictionary<int, List<Vector3>> SpawnPoints = new Dictionary<int, List<Vector3>>();
        public Dictionary<string, List<Vector3>> GunSpawns = new Dictionary<string, List<Vector3>>();


        public Map( Vector3 position, Vector3 size, string name ) {
            Position = position;
            Size = size;
            Name = name;
        }

        public bool IsInZone( Vector3 pos ) {
            return ( pos.X > Position.X - ( Size.X / 2 ) && pos.X < Position.X + ( Size.X / 2 ) && pos.Y > Position.Y - ( Size.Y / 2 ) && pos.Y < Position.Y + ( Size.Y / 2 ) );
        }

        public void AddSpawnPoint( int team, Vector3 spawnPoint ) {
            if( SpawnPoints.ContainsKey( team ) ) {
                SpawnPoints[team].Add( spawnPoint );
            }
            else {
                SpawnPoints.Add( team, new List<Vector3> { spawnPoint } );
            }
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

    }
}
