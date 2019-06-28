using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Client {
    class DriveOrDie : BaseGamemode { 
        public enum Teams {
            Spectators,
            Driver,
            Runnder
        }

        public enum GameState {
            None,
            PreRound,
            InGame,
            PostRound
        }

        public DriveOrDie() {

        }

        public override void Start() {
            base.Start();
        }

        public override void End() {
            base.End();
        }
    }
}
