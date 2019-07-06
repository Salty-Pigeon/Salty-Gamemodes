using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class TTT_DetectiveMenu : SaltyMenu {

        TTT ActiveGame;
        private Func<bool> CanBuy;

        public TTT_DetectiveMenu( TTT activeGame, float x, float y, float width, float height, System.Drawing.Color colour, Action OnClose, Func<bool> canBuy ) : base( x, y, width, height, colour, OnClose ) {
            ActiveGame = activeGame;
            AddSpriteButton("armour", 0.001f, 0.05f, 0.33f, 0.9f, 0, Armour_Click);
            AddSpriteButton("radar", 0.34f, 0.05f, 0.33f, 0.9f, 0, Radar_Click);
            AddSpriteButton( "teleport", 0.67f, 0.05f, 0.33f, 0.9f, 0, Teleport_Click );
            CanBuy = canBuy;
        }

        public void Teleport_Click() {
            if( !CanBuy() ) { return; };
            ActiveGame.CanTeleport = true;
            ActiveGame.WriteChat( "TTT", "Teleport bought", 0, 0, 200 );
            Close();
        }

        public void Radar_Click() {
            if( !CanBuy() ) { return; };
            ActiveGame.SetRadarActive(true);
            ActiveGame.WriteChat( "TTT", "Radar activated", 0, 0, 200 );
            Close();
        }

        public void Armour_Click() {
            if( !CanBuy() ) { return; };
            Game.PlayerPed.Armor = 30;
            ActiveGame.WriteChat( "TTT", "Armour bought", 0, 0, 200 );
            Close();
        }
    }
}
