using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    public class SaltyHUD : HUD {


        public SaltyHUD( Salty gamemode ) : base( gamemode ) {

        }

        public override void Draw() {

            base.Draw();
        }

        public void DrawNoClipWarning( float time ) {
            DrawText2D( 0.5f, 0.3f, "You will return from noclip in " + time + " seconds", 0.5f, 255, 255, 255, 255, true );
        }
    }
}
