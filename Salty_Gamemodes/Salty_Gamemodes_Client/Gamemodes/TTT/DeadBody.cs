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
        public int KillerPed;
        public int KillerID;
        public Vector3 Position;
        public string Name;

        public bool isDiscovered = false;
        public string Caption = "Unidentified body [E]";

        public DeadBody( Vector3 position,int plyID, int killerID ) {
            PlayerPed = GetPlayerPed( plyID );
            Model = (uint)GetEntityModel( PlayerPed );
            KillerPed = GetPlayerPed(killerID);
            PlayerID = plyID;
            KillerID = killerID;
            Name = GetPlayerName( plyID );
            Position = position;
            ID = CreatePed( 4, Model, Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z + 1, 0.0f, true, true );
            
        }

        public void Detective() {

        }

        public void Discovered() {
            isDiscovered = true;
            Caption = Name + "'s dead body";
        }

        public void View() {

        }

        public void Update() {
            SetPedToRagdoll( ID, -1, -1, 0, true, true, true );
        }

    }
}
