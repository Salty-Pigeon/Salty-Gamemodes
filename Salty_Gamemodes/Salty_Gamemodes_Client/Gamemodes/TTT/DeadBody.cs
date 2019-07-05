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

        public int ServerID;
        public int ID = -1; 
        public uint Model;
        public int PlayerPed;
        public int PlayerID;
        public Vector3 Position;
        public string Name;

        public bool isDiscovered = false;
        public string Caption = "Unidentified body [E]";

        public DeadBody( Vector3 position, int plyPed, int plyID ) {
            Model = (uint)GetEntityModel( plyPed );
            PlayerPed = plyPed;
            PlayerID = plyID;
            Name = GetPlayerName( plyID );
            Position = position;
            ID = CreatePed( 4, Model, Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z + 1, 0.0f, true, true );
            
        }

        public void View() {

        }

        public void Update() {
            SetPedToRagdoll( ID, -1, -1, 0, true, true, true );
        }

    }
}
