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

        public TTT_DetectiveMenu( TTT activeGame, float x, float y, float width, float height, System.Drawing.Color colour, Action OnClose ) : base( x, y, width, height, colour, OnClose ) {
            ActiveGame = activeGame;
            AddSpriteButton("armour", 0.001f, 0.05f, 0.235f, 0.9f, 0, Armour_Click);
            AddSpriteButton("radar", 0.236f, 0.05f, 0.235f, 0.9f, 0, Radar_Click);
        }

        public void Teleport_Click() {
            ActiveGame.CanTeleport = true;
        }

        public void Radar_Click() {
            ActiveGame.SetRadarActive(true);
            Close();
        }

        public void Armour_Click() {
            Game.PlayerPed.Armor = 30;
            Close();
        }
    }
}
