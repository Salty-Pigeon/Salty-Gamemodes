using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class TTT_TraitorMenu : SaltyMenu {

        TTT ActiveGame;

        private Func<bool> CanBuy;

        public TTT_TraitorMenu( TTT activeGame, float x, float y, float width, float height, System.Drawing.Color colour, Action OnClose, Func<bool> canBuy ) : base ( x, y, width, height, colour, OnClose ) {
            ActiveGame = activeGame;
            //AddTextButton("Close", 0.9f, 0f, 0.1f, 0.1f, System.Drawing.Color.FromArgb(200, 0, 0), Test_Click);
            AddSpriteButton( "mask", 0f, 0.05f, 0.2f, 0.9f, 0, Disguise_Click );
            AddSpriteButton( "knife", 0.2f, 0.05f, 0.2f, 0.9f, 0, Knife_Click );
            AddSpriteButton( "armour", 0.39f, 0.05f, 0.2f, 0.9f, 0, Armour_Click );
            AddSpriteButton( "radar", 0.6f, 0.05f, 0.2f, 0.9f, 0, Radar_Click );
            AddSpriteButton( "teleport", 0.8f, 0.05f, 0.2f, 0.9f, 0, Teleport_Click );
            CanBuy = canBuy;
        }

        public void Radar_Click() {
            if( !CanBuy() ) { return; }; 
            ActiveGame.GetHUD().SetRadarActive( true );
            ActiveGame.WriteChat( "TTT", "Radar activated", 0, 0, 200 );
            Close();
        }

        public void Teleport_Click() {
            if( !CanBuy() ) { return; };
            ActiveGame.CanTeleport = true;
            ActiveGame.WriteChat( "TTT", "Teleport bought", 0, 0, 200 );
            Close();
        }

        public void Armour_Click() {
            if( !CanBuy() ) { return; };
            Game.PlayerPed.Armor = 30;
            ActiveGame.WriteChat( "TTT", "Armour bought", 0, 0, 200 );
            Close();
        }

        public void Disguise_Click() {
            if( !CanBuy() ) { return; };
            ActiveGame.CanDisguise = true;
            ActiveGame.WriteChat( "TTT", "Disguise bought", 0, 0, 200 );
            Close();
        }

        public void Knife_Click() {
            if( !CanBuy() ) { return; };
            GiveWeaponToPed( PlayerPedId(), (uint)GetHashKey("weapon_knife"), 1, false, false );
            ActiveGame.WriteChat( "TTT", "Knife bought", 0, 0, 200 );
            Close();
        }

    }
}
