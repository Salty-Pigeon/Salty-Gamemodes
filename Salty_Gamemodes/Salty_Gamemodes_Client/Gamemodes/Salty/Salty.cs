using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    public class Salty : BaseGamemode {

        public Salty() {
            ActiveHUD = new SaltyHUD( this );
            SetNoClip( false );
        }

        public SaltyHUD GetHUD() {
            return ActiveHUD as SaltyHUD;
        }

        public override void Update() {

            base.Update();
        }



    }
}
