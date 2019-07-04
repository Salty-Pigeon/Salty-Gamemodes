using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class TTT_Menu : SaltyMenu {


        public TTT_Menu( float x, float y, float width, float height, System.Drawing.Color colour, Action OnClose ) : base ( x, y, width, height, colour, OnClose ) {
            AddTextButton("Close", 0.9f, 0f, 0.1f, 0.1f, System.Drawing.Color.FromArgb(200, 0, 0), Test_Click);
            AddSpriteButton( "mask", 0.02f, 0.1f, 0.2f, 0.4f, 0, Disguise_Click );
            AddSpriteButton( "knife", 0.25f, 0.1f, 0.2f, 0.4f, 0, Knife_Click );
            AddSpriteButton( "armour", 0.5f, 0.1f, 0.2f, 0.4f, 0, Armour_Click );
        }

        public void Armour_Click() {
            Game.PlayerPed.Armor = 30;
            Close();
        }

        public void Disguise_Click() {
            Debug.WriteLine( "Disguise!" );
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
