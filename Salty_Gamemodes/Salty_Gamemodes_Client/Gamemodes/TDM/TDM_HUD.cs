using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class TDM_HUD : HUD {

        TDM ActiveTDM;

        public TDM_HUD( TDM game ) : base( game ) {
            ActiveTDM = game;
        }

        public void DrawXP() {

        }

        public override void Draw() {
            base.Draw();
        }
    }
}
