using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes {
    class Map {
        public int Blip = -1;

        public Vector3 Position;
        public Vector2 Size;

        public bool isActive = false;

        public Map( Vector3 position, Vector2 size ) {
            Position = position;
            Size = size;
            CreateBlimps();
        }

        public void CreateBlimps() {
            //var blip = AddBlipForRadius( Position.X, Position.Y, Position.Z, Radius * 10 );
            Blip = AddBlipForArea( Position.X, Position.Y, Position.Z, Size.X, Size.Y );
            SetBlipAsShortRange( Blip, true );
            SetBlipColour( Blip, 2 );
            SetBlipSprite( Blip, 398 );
            SetBlipRotation( Blip, 0 );
            BeginTextCommandSetBlipName( "STRING" );
            AddTextComponentString( "Map bounds" );
            EndTextCommandSetBlipName( Blip );
            
        }

        public void ClearBlip() {
            RemoveBlip( ref Blip );
        }

        public bool IsInZone() {
            Ped playerPed = Game.PlayerPed;
            Vector3 pos = Game.Player.Character.Position;
            return ( pos.X > Position.X - ( Size.X / 2 ) && pos.X < Position.X + ( Size.X / 2 ) && pos.Y > Position.Y - ( Size.Y / 2 ) && pos.Y < Position.Y + ( Size.Y / 2 ) );
        }
    }
}
