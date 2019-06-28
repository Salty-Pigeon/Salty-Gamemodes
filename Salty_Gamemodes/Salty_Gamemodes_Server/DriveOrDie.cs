using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salty_Gamemodes_Server {
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

        public DriveOrDie( Map gameMap, PlayerList players, int ID ) : base( ID ) {
            GameMap = gameMap;
            this.players = players;
        }

        public override void Start() {

        }

        public override void End() {

        }
    }
}
