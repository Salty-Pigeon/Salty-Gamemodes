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
        public Vector2 Size;

        public List<Vector3> SpawnPoints = new List<Vector3>();


        public Map( Vector3 position, Vector2 size, string name ) {
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
    }
}
