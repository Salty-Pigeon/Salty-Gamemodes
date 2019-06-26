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

        public List<Vector3> SpawnPoints = new List<Vector3>();
        public Dictionary<string, List<Vector3>> GunSpawns = new Dictionary<string, List<Vector3>>();


        public Map( Vector3 position, Vector3 size, string name ) {
            Position = position;
            Size = size;
            Name = name;
        }

        public bool IsInZone( Vector3 pos ) {
            return ( pos.X > Position.X - ( Size.X / 2 ) && pos.X < Position.X + ( Size.X / 2 ) && pos.Y > Position.Y - ( Size.Y / 2 ) && pos.Y < Position.Y + ( Size.Y / 2 ) );
        }

        public void AddSpawnPoint( Vector3 spawnPoint ) {
            SpawnPoints.Add( spawnPoint );
        }

        public void DeleteSpawnPoint( Vector3 spawnPoint ) {
            SpawnPoints.Remove( spawnPoint );
        }


        public void AddWeaponSpawn( string weaponType, Vector3 spawnPoint ) {
            if( GunSpawns.ContainsKey(weaponType) ) {
                GunSpawns[weaponType].Add( spawnPoint );
            } else {
                GunSpawns.Add( weaponType, new List<Vector3> { spawnPoint } );
            }
        }

        public void DeleteWeaponSpawn( Vector3 pos ) {
            foreach( var weapSpawns in GunSpawns ) {
                if( weapSpawns.Value.Contains(pos) ) {
                    GunSpawns[weapSpawns.Key].Remove( pos );
                    break;
                }
            }
        }

        public string SpawnPointsAsString( ) {
            string spawnPoints = "";
            foreach( var vector in SpawnPoints ) {
                spawnPoints += string.Format( "{0},{1},{2}:", vector.X, vector.Y, vector.Z );
            }
            if( spawnPoints == "" ) {
                spawnPoints = "0,0,0:";
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
