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
        }
    }
}
