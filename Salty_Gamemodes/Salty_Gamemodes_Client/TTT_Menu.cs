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


        public TTT_Menu( float x, float y, float width, float height, System.Drawing.Color colour ) : base ( x, y, width, height, colour ) {
            AddTextButton("Test", 0.5f, 0.5f, 0.2f, 0.2f, System.Drawing.Color.FromArgb(200, 0, 0), Test_Click);
        }

        public void Test_Click() {
            Debug.WriteLine("Test was clicked");
        }

    }
}
