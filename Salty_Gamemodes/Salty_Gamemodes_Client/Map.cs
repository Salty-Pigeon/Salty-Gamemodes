using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    public class Map {
        public int Blip = -1;

        public Vector3 Position;
        public Vector3 Size;
        public string Name;

        public List<Vector3> SpawnPoints = new List<Vector3>();

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

        public void ClearBlip() {
            RemoveBlip( ref Blip );
            isVisible = false;
        }

        public bool IsInZone( Vector3 pos ) {
            return ( pos.X > Position.X - ( Size.X / 2 ) && pos.X < Position.X + ( Size.X / 2 ) && pos.Y > Position.Y - ( Size.Y / 2 ) && pos.Y < Position.Y + ( Size.Y / 2 ) );
        }
    }
}
