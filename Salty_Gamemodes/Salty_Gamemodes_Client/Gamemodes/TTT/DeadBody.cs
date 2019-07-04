using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    public class DeadBody : BaseScript {

        public int ID = -1; 
        public uint Model;
        public Vector3 Position;

        public DeadBody( Vector3 position, uint model ) {
            Model = model;
            Position = position;
            ID = CreatePed( 4, model, Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z + 1, 0.0f, true, true );
            
        }

        public void Update() {

        }

    }
}
