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

        public TTT_TraitorMenu( TTT activeGame, float x, float y, float width, float height, System.Drawing.Color colour, Action OnClose ) : base ( x, y, width, height, colour, OnClose ) {
            ActiveGame = activeGame;
            //AddTextButton("Close", 0.9f, 0f, 0.1f, 0.1f, System.Drawing.Color.FromArgb(200, 0, 0), Test_Click);
            AddSpriteButton( "mask", 0.01f, 0.05f, 0.235f, 0.9f, 0, Disguise_Click );
            AddSpriteButton( "knife", 0.2351f, 0.05f, 0.235f, 0.9f, 0, Knife_Click );
            AddSpriteButton( "armour", 0.471f, 0.05f, 0.235f, 0.9f, 0, Armour_Click );
            AddSpriteButton( "radar", 0.751f, 0.05f, 0.235f, 0.9f, 0, Radar_Click );
        }

        public void Radar_Click() {
            ActiveGame.SetRadarActive( true );
            Close();
        }

        public void Armour_Click() {
            Game.PlayerPed.Armor = 30;
            Close();
        }

        public void Disguise_Click() {
            TriggerServerEvent("salty::netUpdatePlayerBool", "disguised");
            Close();
        }

        public void Knife_Click() {
            GiveWeaponToPed( PlayerPedId(), (uint)GetHashKey("weapon_knife"), 1, false, false );
            Close();
        }
        public void Test_Click() {
            Close();
        }

    }
}
